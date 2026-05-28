using Entities.Dtos.AppUserDtos;
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
        private readonly UserLogic _userLogic;

        public CurrentUserGraphSyncService(UserLogic userLogic)
        {
            _userLogic = userLogic;
        }

        public async Task<UserContextDto> SyncCurrentUserFromGraphAsync(string currentUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                throw new ArgumentException("Current user id is required.", nameof(currentUserId));
            }

            await _userLogic.RefreshUserProfileAsync(currentUserId, cancellationToken);

            var hierarchy = await _userLogic.GetCurrentUserHierarchyAsync(
                currentUserId,
                cancellationToken);

            var profile = _userLogic.GetUserProfile(currentUserId);

            var isManager = hierarchy.DirectReports.Any();

            return new UserContextDto
            {
                Profile = profile,
                Hierarchy = hierarchy,
                IsManager = isManager,
                Roles = isManager
                    ? new[] { "Manager" }
                    : Array.Empty<string>(),
                LastGraphSyncAtUtc = DateTime.UtcNow
            };
        }
    }
}
