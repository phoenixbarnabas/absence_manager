using Data;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Logic.Helper
{
    public static class UserActivityLogOutcomes
    {
        public const string Success = "Success";
        public const string Failed = "Failed";
        public const string Forbidden = "Forbidden";
    }

    public static class UserActivityLogActions
    {
        public const string OfficeBookingCreated = "OfficeBookingCreated";
        public const string OfficeBookingCancelled = "OfficeBookingCancelled";

        public const string AbsenceRequestCreated = "AbsenceRequestCreated";
        public const string AbsenceRequestCancelled = "AbsenceRequestCancelled";
        public const string AbsenceRequestApproved = "AbsenceRequestApproved";
        public const string AbsenceRequestRejected = "AbsenceRequestRejected";

        public const string UserGraphSyncCompleted = "UserGraphSyncCompleted";
        public const string UserGraphSyncFailed = "UserGraphSyncFailed";

        public const string UnauthorizedActionAttempt = "UnauthorizedActionAttempt";
    }

    public static class UserActivityLogEntityTypes
    {
        public const string OfficeBooking = "OfficeBooking";
        public const string AbsenceRequest = "AbsenceRequest";
        public const string AppUser = "AppUser";
    }

    public interface IUserActivityLogger
    {
        Task LogAsync(
            string action,
            string entityType,
            string? entityId,
            string? actorUserId,
            object? metadata = null,
            string outcome = UserActivityLogOutcomes.Success,
            CancellationToken cancellationToken = default);
    }

    public class UserActivityLogger : IUserActivityLogger
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly AbsenceManagerDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserActivityLogger> _logger;

        public UserActivityLogger(AbsenceManagerDbContext dbContext, IHttpContextAccessor httpContextAccessor, ICurrentUserService currentUserService, ILogger<UserActivityLogger> logger)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task LogAsync(string action, string entityType, string? entityId, string? actorUserId, object? metadata = null, string outcome = UserActivityLogOutcomes.Success, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                var log = new UserActivityLog
                {
                    CreatedAtUtc = DateTime.UtcNow,
                    ActorUserId = actorUserId,
                    ActorEntraObjectId = _currentUserService.GetEntraObjectId(),
                    TenantId = _currentUserService.GetTenantId(),
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Outcome = outcome,
                    IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    RequestMethod = httpContext?.Request.Method,
                    RequestPath = httpContext?.Request.Path.Value,
                    CorrelationId = httpContext?.TraceIdentifier,
                    MetadataJson = metadata == null
                        ? null
                        : JsonSerializer.Serialize(metadata, JsonOptions)
                };

                _dbContext.UserActivityLogs.Add(log);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "User activity audit log write failed. Action={Action}, EntityType={EntityType}, EntityId={EntityId}, ActorUserId={ActorUserId}",
                    action,
                    entityType,
                    entityId,
                    actorUserId);
            }
        }
    }
}
