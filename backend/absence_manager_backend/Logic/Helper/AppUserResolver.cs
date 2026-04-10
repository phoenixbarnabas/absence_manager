using Data;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
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
        var entraObjectId = principal.FindFirstValue("oid")
            ?? throw new UnauthorizedAccessException("Missing 'oid' claim.");

        var tenantId = principal.FindFirstValue("tid")
            ?? throw new UnauthorizedAccessException("Missing 'tid' claim.");

        var displayName =
            principal.FindFirstValue("name") ??
            principal.FindFirstValue(ClaimTypes.Name) ??
            "Unknown user";

        var email =
            principal.FindFirstValue("preferred_username") ??
            principal.FindFirstValue(ClaimTypes.Email) ??
            principal.FindFirstValue("email");

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