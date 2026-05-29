using Data;
using Entities.Dtos.UserActivityLogDtos;
using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/user-activity-logs")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class UserActivityLogsController : ControllerBase
    {
        private const int DefaultPageSize = 50;
        private const int MaximumPageSize = 200;

        private readonly AbsenceManagerDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public UserActivityLogsController(AbsenceManagerDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] string? actorUserId,
            [FromQuery] string? action,
            [FromQuery] string? entityType,
            [FromQuery] string? entityId,
            [FromQuery] string? outcome,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync(cancellationToken);
                var canSeeAllLogs = AbsenceRequestLogic.IsHrOrAdmin(currentUser);

                page = Math.Max(page, 1);
                pageSize = Math.Clamp(pageSize, 1, MaximumPageSize);

                var query =
                    from log in _dbContext.UserActivityLogs.AsNoTracking()
                    join user in _dbContext.AppUsers.AsNoTracking()
                        on log.ActorUserId equals user.Id into userJoin
                    from actor in userJoin.DefaultIfEmpty()
                    select new
                    {
                        Log = log,
                        Actor = actor
                    };

                if (!canSeeAllLogs)
                {
                    query = query.Where(x => x.Log.ActorUserId == currentUser.Id);
                }
                else if (!string.IsNullOrWhiteSpace(actorUserId))
                {
                    query = query.Where(x => x.Log.ActorUserId == actorUserId);
                }

                if (fromUtc.HasValue)
                {
                    query = query.Where(x => x.Log.CreatedAtUtc >= fromUtc.Value);
                }

                if (toUtc.HasValue)
                {
                    query = query.Where(x => x.Log.CreatedAtUtc <= toUtc.Value);
                }

                if (!string.IsNullOrWhiteSpace(action))
                {
                    query = query.Where(x => x.Log.Action == action);
                }

                if (!string.IsNullOrWhiteSpace(entityType))
                {
                    query = query.Where(x => x.Log.EntityType == entityType);
                }

                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    query = query.Where(x => x.Log.EntityId == entityId);
                }

                if (!string.IsNullOrWhiteSpace(outcome))
                {
                    query = query.Where(x => x.Log.Outcome == outcome);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .OrderByDescending(x => x.Log.CreatedAtUtc)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new UserActivityLogViewDto
                    {
                        Id = x.Log.Id,
                        CreatedAtUtc = x.Log.CreatedAtUtc,
                        ActorUserId = x.Log.ActorUserId,
                        ActorDisplayName = x.Actor != null ? x.Actor.DisplayName : null,
                        ActorEmail = x.Actor != null ? x.Actor.Email : null,
                        ActorEntraObjectId = x.Log.ActorEntraObjectId,
                        TenantId = x.Log.TenantId,
                        Action = x.Log.Action,
                        EntityType = x.Log.EntityType,
                        EntityId = x.Log.EntityId,
                        Outcome = x.Log.Outcome,
                        IpAddress = x.Log.IpAddress,
                        UserAgent = x.Log.UserAgent,
                        RequestMethod = x.Log.RequestMethod,
                        RequestPath = x.Log.RequestPath,
                        CorrelationId = x.Log.CorrelationId,
                        MetadataJson = x.Log.MetadataJson
                    })
                    .ToListAsync(cancellationToken);

                return Ok(new UserActivityLogListResultDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    Items = items
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (OperationCanceledException)
            {
                return new StatusCodeResult(499);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync(cancellationToken);
                var canSeeAllLogs = AbsenceRequestLogic.IsHrOrAdmin(currentUser);

                var result = await (
                    from log in _dbContext.UserActivityLogs.AsNoTracking()
                    join user in _dbContext.AppUsers.AsNoTracking()
                        on log.ActorUserId equals user.Id into userJoin
                    from actor in userJoin.DefaultIfEmpty()
                    where log.Id == id
                    select new UserActivityLogViewDto
                    {
                        Id = log.Id,
                        CreatedAtUtc = log.CreatedAtUtc,
                        ActorUserId = log.ActorUserId,
                        ActorDisplayName = actor != null ? actor.DisplayName : null,
                        ActorEmail = actor != null ? actor.Email : null,
                        ActorEntraObjectId = log.ActorEntraObjectId,
                        TenantId = log.TenantId,
                        Action = log.Action,
                        EntityType = log.EntityType,
                        EntityId = log.EntityId,
                        Outcome = log.Outcome,
                        IpAddress = log.IpAddress,
                        UserAgent = log.UserAgent,
                        RequestMethod = log.RequestMethod,
                        RequestPath = log.RequestPath,
                        CorrelationId = log.CorrelationId,
                        MetadataJson = log.MetadataJson
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (result == null)
                {
                    return NotFound(new { message = "User activity log not found." });
                }

                if (!canSeeAllLogs && result.ActorUserId != currentUser.Id)
                {
                    return Forbid();
                }

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
            catch (OperationCanceledException)
            {
                return new StatusCodeResult(499);
            }
        }

        private async Task<Entities.Models.AppUser> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);

            return await _dbContext.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                ?? throw new KeyNotFoundException("Current user not found.");
        }
    }
}
