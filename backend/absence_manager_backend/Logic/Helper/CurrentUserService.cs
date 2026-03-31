using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Logic.Helper;

public interface ICurrentUserService
{
    string GetUserId();
    string? GetEntraObjectId();
    string? GetTenantId();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("Missing user id claim.");
    }

    public string? GetEntraObjectId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst("entra_oid")?.Value;
    }

    public string? GetTenantId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
    }
}