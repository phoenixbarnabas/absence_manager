using Entities.Dtos.AppUserDtos;
using Logic.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Logic
{
    public interface ICurrentUserGraphSyncService
    {
        Task<UserContextDto> SyncCurrentUserFromGraphAsync(string currentUserId, CancellationToken cancellationToken = default);
    }

    public class CurrentUserGraphSyncService : ICurrentUserGraphSyncService
    {
        private readonly IUserLogic _userLogic;
        private readonly IUserActivityLogger _activityLogger;

        public CurrentUserGraphSyncService(IUserLogic userLogic, IUserActivityLogger activityLogger)
        {
            _userLogic = userLogic;
            _activityLogger = activityLogger;
        }

        public async Task<UserContextDto> SyncCurrentUserFromGraphAsync(string currentUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                throw new ArgumentException("Current user id is required.", nameof(currentUserId));
            }

            var startedAtUtc = DateTime.UtcNow;

            try
            {
                await _userLogic.RefreshUserProfileAsync(currentUserId, cancellationToken);

                var hierarchy = await _userLogic.SyncCurrentUserHierarchyFromGraphAsync(
                    currentUserId,
                    cancellationToken);

                var profile = _userLogic.GetUserProfile(currentUserId);

                var isManager = hierarchy.DirectReports.Any();

                var result = new UserContextDto
                {
                    Profile = profile,
                    Hierarchy = hierarchy,
                    IsManager = isManager,
                    Roles = isManager
                        ? new[] { "Manager" }
                        : Array.Empty<string>(),
                    LastGraphSyncAtUtc = DateTime.UtcNow
                };

                await _activityLogger.LogAsync(
                    action: "UserGraphSyncCompleted",
                    entityType: "AppUser",
                    entityId: currentUserId,
                    actorUserId: currentUserId,
                    metadata: new
                    {
                        HasManager = hierarchy.Manager != null,
                        DirectReportsCount = hierarchy.DirectReports.Count,
                        IsManager = isManager,
                        DurationMs = (int)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds
                    },
                    cancellationToken: cancellationToken);

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _activityLogger.LogAsync(
                    action: "UserGraphSyncFailed",
                    entityType: "AppUser",
                    entityId: currentUserId,
                    actorUserId: currentUserId,
                    metadata: new
                    {
                        ErrorType = ex.GetType().Name,
                        ErrorMessage = ex.Message,
                        DurationMs = (int)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds
                    },
                    outcome: UserActivityLogOutcomes.Failed,
                    cancellationToken: CancellationToken.None);

                throw;
            }
        }
    }
}
