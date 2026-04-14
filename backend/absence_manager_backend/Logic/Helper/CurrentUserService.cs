using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Logic.Helper;

public interface ICurrentUserService
{
    Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
    string? GetEntraObjectId();
    string? GetTenantId();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppUserResolver _appUserResolver;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IAppUserResolver appUserResolver)
    {
        _httpContextAccessor = httpContextAccessor;
        _appUserResolver = appUserResolver;
    }

    public async Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("Missing HttpContext user.");

        var user = await _appUserResolver.ResolveCurrentUserAsync(principal, cancellationToken);
        return user.Id;
    }

    public string? GetEntraObjectId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue("oid")
            ?? user?.FindFirstValue(ClaimConstants.ObjectId)
            ?? user?.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
    }

    public string? GetTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue("tid")
            ?? user?.FindFirstValue(ClaimConstants.TenantId)
            ?? user?.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
    }
}