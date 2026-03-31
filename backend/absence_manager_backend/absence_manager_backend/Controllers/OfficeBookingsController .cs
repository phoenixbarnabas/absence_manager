using Data;
using Entities.Dtos.OfficeBooking;
using Entities.Models;
using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/office-bookings")]
    [Authorize]
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
        public IActionResult GetAvailability([FromQuery] string officeId, [FromQuery] DateOnly date)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
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
        public IActionResult GetDaySummaries([FromQuery] string officeId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
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
                var currentUserId = _currentUserService.GetUserId();
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
                var currentUserId = _currentUserService.GetUserId();
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
        }

        [HttpDelete("{bookingId}")]
        public IActionResult CancelBooking(string bookingId)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                _officeBookingLogic.CancelBooking(bookingId, currentUserId, isAdmin: false);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
