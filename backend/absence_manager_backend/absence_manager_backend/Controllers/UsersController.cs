using Logic.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserLogic _userLogic;

        public UsersController(UserLogic userLogic)
        {
            _userLogic = userLogic;
        }

        [HttpGet("{userId}/profile")]
        public IActionResult GetUserProfile(string userId)
        {
            try
            {
                var result = _userLogic.GetUserProfile(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
