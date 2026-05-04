using Entities.Dtos.AbsenceRequestDtos;
using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/absence-requests")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class AbsenceRequestsController : ControllerBase
    {
        private readonly AbsenceRequestLogic _absenceRequestLogic;
        private readonly ICurrentUserService _currentUserService;

        public AbsenceRequestsController(
            AbsenceRequestLogic absenceRequestLogic,
            ICurrentUserService currentUserService)
        {
            _absenceRequestLogic = absenceRequestLogic;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateAbsenceRequestDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = await _absenceRequestLogic.CreateAsync(dto, currentUserId, cancellationToken);
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMine(
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = await _absenceRequestLogic.GetMineAsync(currentUserId, fromDate, toDate, cancellationToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            string id,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = await _absenceRequestLogic.GetByIdAsync(id, currentUserId, cancellationToken);
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
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(
            string id,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                await _absenceRequestLogic.CancelAsync(id, currentUserId, cancellationToken);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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