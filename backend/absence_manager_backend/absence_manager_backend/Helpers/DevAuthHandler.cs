using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Data;
using Entities.Models;

namespace absence_manager_backend.Helpers
{
    public class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AbsenceManagerDbContext _dbContext;

        public DevAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            AbsenceManagerDbContext dbContext)
            : base(options, logger, encoder)
        {
            _dbContext = dbContext;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Próbálkozz először Entra Object ID-vel (Entra ID integráció)
            var entraObjectId = Request.Headers["X-Entra-ObjectId"].FirstOrDefault();

            // Fallback: Dev User ID header (fejlesztéshez, seed adatokkal)
            var devUserId = Request.Headers["X-Dev-UserId"].FirstOrDefault();

            // Válassz az elérhető headerek közül
            AppUser user = null;

            if (!string.IsNullOrWhiteSpace(entraObjectId))
            {
                // Entra ID alapú keresés
                user = _dbContext.AppUsers.FirstOrDefault(x => 
                    x.EntraObjectId == entraObjectId && x.IsActive);

                if (user == null)
                {
                    return Task.FromResult(AuthenticateResult.Fail($"User with Entra ObjectId '{entraObjectId}' not found or is inactive."));
                }
            }
            else if (!string.IsNullOrWhiteSpace(devUserId))
            {
                // Development User ID alapú keresés (seed adatok)
                user = _dbContext.AppUsers.FirstOrDefault(x => 
                    x.Id == devUserId && x.IsActive);

                if (user == null)
                {
                    return Task.FromResult(AuthenticateResult.Fail($"Dev user with ID '{devUserId}' not found or is inactive."));
                }
            }
            else
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing X-Entra-ObjectId or X-Dev-UserId header."));
            }

            // Claims létrehozása az eredményes felhasználóhoz
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.DisplayName),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty),
                new System.Security.Claims.Claim("entra_oid", user.EntraObjectId ?? string.Empty),
                new System.Security.Claims.Claim("tenant_id", user.TenantId ?? string.Empty),
                new System.Security.Claims.Claim("department", user.Department ?? string.Empty),
                new System.Security.Claims.Claim("job_title", user.JobTitle ?? string.Empty)
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
