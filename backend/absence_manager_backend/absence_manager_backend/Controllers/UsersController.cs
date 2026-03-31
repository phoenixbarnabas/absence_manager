using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
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
        public IActionResult GetMe()
        {
            var currentUserId = _currentUserService.GetUserId();
            var result = _userLogic.GetUserProfile(currentUserId);
            return Ok(result);
        }
    }
}