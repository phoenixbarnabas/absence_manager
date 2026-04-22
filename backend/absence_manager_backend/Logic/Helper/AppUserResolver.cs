using Data;
using Entities.Dtos.Graph;
using Entities.Models;
using Logic.Logic;
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
    private readonly IMsGraphLogic _graphLogic;

    public AppUserResolver(AbsenceManagerDbContext dbContext, IMsGraphLogic graphLogic)
    {
        _dbContext = dbContext;
        _graphLogic = graphLogic;

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

        GraphUserProfileDto? graphProfile = null;

        try
        {
            graphProfile = await _graphLogic.GetCurrentUserProfileAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Graph profile retrieval failed: {ex.Message}");
        }

        var resolvedDisplayName = graphProfile?.DisplayName ?? displayName;
        var resolvedEmail = graphProfile?.Email ?? email;
        var resolvedDepartment = graphProfile?.Department ?? string.Empty;
        var resolvedJobTitle = graphProfile?.JobTitle ?? string.Empty;

        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(
                x => x.EntraObjectId == entraObjectId && x.TenantId == tenantId,
                cancellationToken);

        if (user != null)
        {
            var changed = false;

            if (user.DisplayName != resolvedDisplayName)
            {
                user.DisplayName = resolvedDisplayName;
                changed = true;
            }

            if (user.Email != resolvedEmail)
            {
                user.Email = resolvedEmail;
                changed = true;
            }

            if (user.Department != resolvedDepartment)
            {
                user.Department = resolvedDepartment;
                changed = true;
            }

            if (user.JobTitle != resolvedJobTitle)
            {
                user.JobTitle = resolvedJobTitle;
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
            DisplayName = resolvedDisplayName,
            Email = resolvedEmail,
            Department = resolvedDepartment,
            JobTitle = resolvedJobTitle,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AppUsers.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}