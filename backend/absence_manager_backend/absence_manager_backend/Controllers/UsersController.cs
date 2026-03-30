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

        [HttpGet("{userId}")]
        public IActionResult GetById(string userId)
        {
            try
            {
                var result = _userLogic.GetUserProfile(userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
