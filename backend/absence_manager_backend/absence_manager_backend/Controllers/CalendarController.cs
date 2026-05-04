using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/calendar")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarLogic _calendarLogic;
        private readonly ICurrentUserService _currentUserService;

        public CalendarController(
            CalendarLogic calendarLogic,
            ICurrentUserService currentUserService)
        {
            _calendarLogic = calendarLogic;
            _currentUserService = currentUserService;
        }

        [HttpGet("day-infos")]
        public IActionResult GetDayInfos(
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate)
        {
            try
            {
                var result = _calendarLogic.GetDayInfos(fromDate, toDate);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate,
            [FromQuery] string scope,
            [FromQuery] string[] eventTypes,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = _calendarLogic.GetEvents(fromDate, toDate, currentUserId, scope, eventTypes);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
