using Microsoft.AspNetCore.Http;
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
        return _httpContextAccessor.HttpContext?.User.FindFirst("oid")?.Value;
    }

    public string? GetTenantId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst("tid")?.Value;
    }
}