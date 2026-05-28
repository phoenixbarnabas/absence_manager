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
        private readonly IUserLogic _userLogic;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentUserGraphSyncService _currentUserGraphSyncService;

        public UsersController(IUserLogic userLogic, ICurrentUserService currentUserService, ICurrentUserGraphSyncService currentUserGraphSyncService)
        {
            _userLogic = userLogic;
            _currentUserService = currentUserService;
            _currentUserGraphSyncService = currentUserGraphSyncService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = _userLogic.GetUserProfile(currentUserId);
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

        [HttpGet("me/manager")]
        public async Task<IActionResult> GetMyManager(CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = await _userLogic.GetCurrentUserManagerAsync(currentUserId, cancellationToken);

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

        [HttpGet("me/direct-reports")]
        public async Task<IActionResult> GetMyDirectReports(CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = await _userLogic.GetCurrentUserDirectReportsAsync(currentUserId, cancellationToken);

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

        [HttpGet("me/hierarchy")]
        public async Task<IActionResult> GetMyHierarchy(CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
                var result = await _userLogic.GetCurrentUserHierarchyFromLocalDbAsync(
                    currentUserId,
                    cancellationToken);

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

        //[HttpPost("me/sync-graph-profile")]
        //public async Task<IActionResult> SyncMyGraphProfile(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
        //        await _userLogic.RefreshUserProfileAsync(currentUserId, cancellationToken);

        //        return NoContent();
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        return StatusCode(499, new { message = "A kérés megszakadt." });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpPost("me/sync-from-graph")]
        public async Task<IActionResult> SyncCurrentUserFromGraph(CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);

                var result = await _currentUserGraphSyncService.SyncCurrentUserFromGraphAsync(
                    currentUserId,
                    cancellationToken);

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { message = "A kérés megszakadt." });
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}