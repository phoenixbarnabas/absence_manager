using Data;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Logic.Helper;

public interface IAppUserResolver
{
    Task<AppUser> ResolveCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public class AppUserResolver : IAppUserResolver
{
    private readonly AbsenceManagerDbContext _dbContext;

    public AppUserResolver(AbsenceManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppUser> ResolveCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        string? FindClaim(params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var value = principal.FindFirstValue(claimType);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        var entraObjectId = FindClaim(
            "oid",
            ClaimConstants.ObjectId,
            "http://schemas.microsoft.com/identity/claims/objectidentifier"
        );

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            var availableClaims = string.Join(
                ", ",
                principal.Claims.Select(c => $"{c.Type}={c.Value}")
            );

            throw new UnauthorizedAccessException(
                $"Missing 'oid' claim. Available claims: {availableClaims}");
        }

        var tenantId = FindClaim(
            "tid",
            ClaimConstants.TenantId,
            "http://schemas.microsoft.com/identity/claims/tenantid"
        );

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            var availableClaims = string.Join(
                ", ",
                principal.Claims.Select(c => $"{c.Type}={c.Value}")
            );

            throw new UnauthorizedAccessException(
                $"Missing 'tid' claim. Available claims: {availableClaims}");
        }

        var displayName = FindClaim(
            "name",
            ClaimConstants.Name,
            ClaimTypes.Name
        ) ?? "Unknown user";

        var email = FindClaim(
            "preferred_username",
            ClaimConstants.PreferredUserName,
            ClaimTypes.Email,
            "email",
            "upn"
        );

        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(
                x => x.EntraObjectId == entraObjectId && x.TenantId == tenantId,
                cancellationToken);

        if (user != null)
        {
            var changed = false;

            if (user.DisplayName != displayName)
            {
                user.DisplayName = displayName;
                changed = true;
            }

            if (user.Email != email)
            {
                user.Email = email;
                changed = true;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return user;
        }

        user = new AppUser
        {
            EntraObjectId = entraObjectId,
            TenantId = tenantId,
            DisplayName = displayName,
            Email = email,
            Department = string.Empty,
            JobTitle = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AppUsers.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}