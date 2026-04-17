using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/auth-debug")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class AuthDebugController : ControllerBase
    {
        [HttpGet("me")]
        public IActionResult Me()
        {
            string? Find(params string[] claimTypes)
            {
                foreach (var type in claimTypes)
                {
                    var value = User.FindFirstValue(type);
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
                return null;
            }

            return Ok(new
            {
                oid = Find("oid", ClaimConstants.ObjectId),
                tid = Find("tid", ClaimConstants.TenantId),
                name = Find("name", ClaimConstants.Name),
                preferred_username = Find("preferred_username", ClaimConstants.PreferredUserName),
                email = Find("email", ClaimTypes.Email),
                upn = Find("upn"),
                scp = Find("scp", ClaimConstants.Scp, ClaimConstants.Scope),
                roles = User.FindAll("roles")
                            .Concat(User.FindAll(ClaimConstants.Role))
                            .Select(x => x.Value)
                            .Distinct()
                            .ToList()
            });
        }

        [HttpGet("claims")]
        public IActionResult Claims()
        {
            var claims = User.Claims
                .Select(c => new
                {
                    c.Type,
                    c.Value
                })
                .OrderBy(x => x.Type)
                .ToList();

            return Ok(claims);
        }
    }
}