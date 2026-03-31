using Data;
using Entities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/dev-auth")]
    public class DevAuthController : ControllerBase
    {
        private readonly AbsenceManagerDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly IWebHostEnvironment _environment;

        public DevAuthController(
            AbsenceManagerDbContext dbContext,
            IOptions<JwtSettings> jwtSettings,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtSettings.Value;
            _environment = environment;
        }

        [AllowAnonymous]
        [HttpPost("login/{userId}")]
        public IActionResult LoginAsSeedUser(string userId)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            var user = _dbContext.AppUsers.FirstOrDefault(x => x.Id == userId && x.IsActive);

            if (user == null)
                return NotFound(new { message = $"Active seed user not found: {userId}" });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("entra_oid", user.EntraObjectId ?? string.Empty),
                new Claim("tenant_id", user.TenantId ?? string.Empty),
                new Claim("department", user.Department ?? string.Empty),
                new Claim("job_title", user.JobTitle ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = expires,
                user = new
                {
                    user.Id,
                    user.DisplayName,
                    user.Email,
                    user.EntraObjectId,
                    user.TenantId,
                    user.Department,
                    user.JobTitle
                }
            });
        }

        [AllowAnonymous]
        [HttpGet("seed-users")]
        public IActionResult GetSeedUsers()
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            var users = _dbContext.AppUsers
                .Where(x => x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.DisplayName,
                    x.Email,
                    x.EntraObjectId
                })
                .ToList();

            return Ok(users);
        }
    }
}
