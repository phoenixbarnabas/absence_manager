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
    Task<AppUser> ResolveCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}

public class AppUserResolver : IAppUserResolver
{
    private readonly AbsenceManagerDbContext _dbContext;

    public AppUserResolver(AbsenceManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppUser> ResolveCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
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
            "entra_oid",
            ClaimConstants.ObjectId,
            "http://schemas.microsoft.com/identity/claims/objectidentifier"
        );

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            throw new UnauthorizedAccessException("Missing Entra object id claim.");
        }

        var tenantId = FindClaim(
            "tid",
            "tenant_id",
            ClaimConstants.TenantId,
            "http://schemas.microsoft.com/identity/claims/tenantid"
        );

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new UnauthorizedAccessException("Missing tenant id claim.");
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

        var department = FindClaim("department") ?? string.Empty;
        var jobTitle = FindClaim("job_title", "jobTitle") ?? string.Empty;

        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(
                x => x.EntraObjectId == entraObjectId && x.TenantId == tenantId,
                cancellationToken);

        if (user != null)
        {
            var changed = false;

            if (!string.IsNullOrWhiteSpace(displayName) && user.DisplayName != displayName)
            {
                user.DisplayName = displayName;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(email) && user.Email != email)
            {
                user.Email = email;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(department) && user.Department != department)
            {
                user.Department = department;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(jobTitle) && user.JobTitle != jobTitle)
            {
                user.JobTitle = jobTitle;
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
            Department = department,
            JobTitle = jobTitle,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AppUsers.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(user).State = EntityState.Detached;

            var existingUser = await _dbContext.AppUsers
                .FirstOrDefaultAsync(
                    x => x.EntraObjectId == entraObjectId && x.TenantId == tenantId,
                    cancellationToken);

            if (existingUser != null)
            {
                return existingUser;
            }

            throw;
        }
    }
}