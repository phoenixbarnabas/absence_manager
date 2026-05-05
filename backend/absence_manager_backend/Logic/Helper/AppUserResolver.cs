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

        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(
                x => x.EntraObjectId == entraObjectId && x.TenantId == tenantId,
                cancellationToken);

        if (user != null)
        {
            try
            {
                var existingGraphProfile = await _graphLogic.GetCurrentUserProfileAsync(cancellationToken);

                user.DisplayName = existingGraphProfile?.DisplayName ?? user.DisplayName;
                user.Email = existingGraphProfile?.Email ?? user.Email;
                user.Department = existingGraphProfile?.Department ?? user.Department;
                user.JobTitle = existingGraphProfile?.JobTitle ?? user.JobTitle;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Graph profile refresh failed: {ex.Message}");
            }

            return user;
        }

        GraphUserProfileDto? graphProfile = null;

        try
        {
            graphProfile = await _graphLogic.GetCurrentUserProfileAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Graph profile retrieval failed: {ex.Message}");
        }

        var resolvedDisplayName = graphProfile?.DisplayName ?? displayName;
        var resolvedEmail = graphProfile?.Email ?? email;
        var resolvedDepartment = graphProfile?.Department ?? string.Empty;
        var resolvedJobTitle = graphProfile?.JobTitle ?? string.Empty;

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