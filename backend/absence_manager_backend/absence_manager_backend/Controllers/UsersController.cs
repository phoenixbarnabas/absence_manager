using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class UsersController : ControllerBase
    {
        private readonly UserLogic _userLogic;
        private readonly ICurrentUserService _currentUserService;

        public UsersController(UserLogic userLogic, ICurrentUserService currentUserService)
        {
            _userLogic = userLogic;
            _currentUserService = currentUserService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        {
            var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
            var result = _userLogic.GetUserProfile(currentUserId);
            return Ok(result);
        }
    }
}