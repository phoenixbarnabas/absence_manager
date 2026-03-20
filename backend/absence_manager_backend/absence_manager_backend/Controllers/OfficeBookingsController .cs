using Entities.Dtos.OfficeBooking;
using Logic.Logic;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/office-bookings")]
    public class OfficeBookingsController : ControllerBase
    {
        private readonly OfficeBookingLogic _officeBookingLogic;

        public OfficeBookingsController(OfficeBookingLogic officeBookingLogic)
        {
            _officeBookingLogic = officeBookingLogic;
        }

        [HttpGet("availability")]
        public IActionResult GetAvailability([FromQuery] string officeId, [FromQuery] DateOnly date)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
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
        public IActionResult GetDaySummaries(
            [FromQuery] string officeId,
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
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
        public IActionResult GetMyBookings([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = _officeBookingLogic.GetMyBookings(currentUserId, fromDate, toDate);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateBooking([FromBody] CreateOfficeBookingDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = _officeBookingLogic.CreateBooking(dto, currentUserId);
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

        [HttpDelete("{bookingId}")]
        public IActionResult CancelBooking(string bookingId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                _officeBookingLogic.CancelBooking(bookingId, currentUserId, isAdmin: false);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? User.FindFirstValue("oid")
                   ?? throw new UnauthorizedAccessException("Current user id could not be resolved.");
        }
    }
}
