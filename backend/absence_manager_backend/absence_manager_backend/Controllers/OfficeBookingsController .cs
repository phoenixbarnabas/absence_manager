using Entities.Dtos.OfficeBooking;
using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/office-bookings")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class OfficeBookingsController : ControllerBase
    {
        private readonly OfficeBookingLogic _officeBookingLogic;
        private readonly ICurrentUserService _currentUserService;

        public OfficeBookingsController(
            OfficeBookingLogic officeBookingLogic,
            ICurrentUserService currentUserService)
        {
            _officeBookingLogic = officeBookingLogic;
            _currentUserService = currentUserService;
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability(
            [FromQuery] string officeId,
            [FromQuery] DateOnly date,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = _officeBookingLogic.GetOfficeDayAvailability(officeId, date, currentUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("day-summaries")]
        public async Task<IActionResult> GetDaySummaries(
            [FromQuery] string officeId,
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = _officeBookingLogic.GetOfficeDaySummaries(officeId, fromDate, toDate, currentUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = _officeBookingLogic.GetMyBookings(currentUserId, fromDate, toDate);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException)
            {
                return new StatusCodeResult(499);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(
            [FromBody] CreateOfficeBookingDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = _officeBookingLogic.CreateBooking(dto, currentUserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (OperationCanceledException)
            {
                return new StatusCodeResult(499);
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> CancelBooking(
            string bookingId,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                _officeBookingLogic.CancelBooking(bookingId, currentUserId, isAdmin: false);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException)
            {
                return new StatusCodeResult(499);
            }
        }
    }
}