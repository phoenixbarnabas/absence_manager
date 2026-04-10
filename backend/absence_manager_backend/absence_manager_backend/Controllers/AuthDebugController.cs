using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            return Ok(new
            {
                oid = User.FindFirstValue("oid"),
                tid = User.FindFirstValue("tid"),
                name = User.FindFirstValue("name"),
                preferred_username = User.FindFirstValue("preferred_username"),
                email = User.FindFirstValue("email"),
                upn = User.FindFirstValue("upn"),
                scp = User.FindFirstValue("scp"),
                roles = User.FindAll("roles").Select(x => x.Value).ToList()
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