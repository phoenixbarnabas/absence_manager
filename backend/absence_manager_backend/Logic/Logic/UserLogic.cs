using Data;
using Entities.Dtos.AppUserDtos;
using Entities.Dtos.Graph;
using Entities.Dtos.WorkStationDtos;
using Entities.Models;
using Logic.Helper;
using Microsoft.EntityFrameworkCore;

namespace Logic.Logic
{
    public interface IUserLogic
    {
        UserProfileDto GetUserProfile(string userId);

        Task<AppUserHierarchyDto> SyncCurrentUserHierarchyFromGraphAsync(string currentUserId, CancellationToken cancellationToken = default);

        Task<AppUserHierarchyDto> GetCurrentUserHierarchyFromLocalDbAsync(string currentUserId, CancellationToken cancellationToken = default);

        Task<GraphAppUserDto?> GetCurrentUserManagerAsync(string currentUserId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GraphAppUserDto>> GetCurrentUserDirectReportsAsync(string currentUserId, CancellationToken cancellationToken = default);

        Task RefreshUserProfileAsync(string userId, CancellationToken cancellationToken);
    }

    public class UserLogic : IUserLogic
    {
        private readonly Repository<AppUser> _userRepository;
        private readonly DtoProvider _dtoProvider;
        private readonly AbsenceManagerDbContext _dbContext;
        private readonly IMsGraphLogic _graphLogic;

        public UserLogic(Repository<AppUser> userRepository, DtoProvider dtoProvider, AbsenceManagerDbContext dbContext, IMsGraphLogic graphLogic)
        {
            _userRepository = userRepository;
            _dtoProvider = dtoProvider;
            _dbContext = dbContext;
            _graphLogic = graphLogic;
        }

        public UserProfileDto GetUserProfile(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            var user = _userRepository.FindById(userId);

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active.");

            return _dtoProvider.Mapper.Map<UserProfileDto>(user);
        }

        public async Task<AppUserHierarchyDto> GetCurrentUserHierarchyAsync(string currentUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new ArgumentException("Current user id is required.", nameof(currentUserId));

            var currentUser = await _dbContext.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found.");

            if (!currentUser.IsActive)
                throw new InvalidOperationException("User is not active.");

            if (string.IsNullOrWhiteSpace(currentUser.EntraObjectId))
                throw new InvalidOperationException("Current user has no Entra object id.");

            var graphHierarchy = await _graphLogic.GetUserHierarchyAsync(
                currentUser.EntraObjectId,
                cancellationToken);

            var graphUsers = new List<GraphUserDto>();

            if (graphHierarchy.CurrentUser != null)
                graphUsers.Add(graphHierarchy.CurrentUser);

            if (graphHierarchy.Manager != null)
                graphUsers.Add(graphHierarchy.Manager);

            graphUsers.AddRange(graphHierarchy.DirectReports);

            var localUsersByEntraObjectId = await GetLocalUsersByEntraObjectIdAsync(
                graphUsers,
                cancellationToken);

            await SyncHierarchyRelationsAsync(
                currentUser,
                graphHierarchy,
                localUsersByEntraObjectId,
                cancellationToken);

            return new AppUserHierarchyDto
            {
                CurrentUser = ToGraphAppUserDto(graphHierarchy.CurrentUser, localUsersByEntraObjectId),
                Manager = ToGraphAppUserDto(graphHierarchy.Manager, localUsersByEntraObjectId),
                DirectReports = graphHierarchy.DirectReports
                    .Select(x => ToGraphAppUserDto(x, localUsersByEntraObjectId))
                    .Where(x => x != null)
                    .Cast<GraphAppUserDto>()
                    .ToList()
            };
        }

        public Task<AppUserHierarchyDto> SyncCurrentUserHierarchyFromGraphAsync(string currentUserId, CancellationToken cancellationToken = default)
        {
            return GetCurrentUserHierarchyAsync(currentUserId, cancellationToken);
        }

        public async Task<AppUserHierarchyDto> GetCurrentUserHierarchyFromLocalDbAsync(string currentUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new ArgumentException("Current user id is required.", nameof(currentUserId));

            var currentUser = await _dbContext.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found.");

            if (!currentUser.IsActive)
                throw new InvalidOperationException("User is not active.");

            var managerRelation = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .Include(x => x.ManagerUser)
                .FirstOrDefaultAsync(x =>
                    x.UserId == currentUserId &&
                    x.IsActive,
                    cancellationToken);

            var directReportRelations = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.ManagerUserId == currentUserId &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

            return new AppUserHierarchyDto
            {
                CurrentUser = ToGraphAppUserDto(currentUser),
                Manager = ToManagerGraphAppUserDto(managerRelation),
                DirectReports = directReportRelations
                    .Where(x => x.User != null && x.User.IsActive)
                    .Select(x => ToGraphAppUserDto(x.User))
                    .ToList()
            };
        }

        public async Task<GraphAppUserDto?> GetCurrentUserManagerAsync(string currentUserId, CancellationToken cancellationToken = default)
        {
            var hierarchy = await GetCurrentUserHierarchyFromLocalDbAsync(
                currentUserId,
                cancellationToken);

            return hierarchy.Manager;
        }

        public async Task<IReadOnlyList<GraphAppUserDto>> GetCurrentUserDirectReportsAsync(string currentUserId, CancellationToken cancellationToken = default)
        {
            var hierarchy = await GetCurrentUserHierarchyFromLocalDbAsync(
                currentUserId,
                cancellationToken);

            return hierarchy.DirectReports;
        }

        public async Task RefreshUserProfileAsync(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            var user = await _dbContext.AppUsers
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!user.IsActive)
            {
                throw new InvalidOperationException("User is not active.");
            }

            var graphProfile = await _graphLogic.GetCurrentUserProfileAsync(cancellationToken);

            if (graphProfile == null)
            {
                return;
            }

            var changed = false;

            if (!string.IsNullOrWhiteSpace(graphProfile.DisplayName) &&
                user.DisplayName != graphProfile.DisplayName)
            {
                user.DisplayName = graphProfile.DisplayName;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(graphProfile.Email) &&
                user.Email != graphProfile.Email)
            {
                user.Email = graphProfile.Email;
                changed = true;
            }

            if (graphProfile.Department != null &&
                user.Department != graphProfile.Department)
            {
                user.Department = graphProfile.Department;
                changed = true;
            }

            if (graphProfile.JobTitle != null &&
                user.JobTitle != graphProfile.JobTitle)
            {
                user.JobTitle = graphProfile.JobTitle;
                changed = true;
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task<Dictionary<string, AppUser>> GetLocalUsersByEntraObjectIdAsync(IEnumerable<GraphUserDto> graphUsers, CancellationToken cancellationToken)
        {
            var entraObjectIds = graphUsers
                .Where(x => !string.IsNullOrWhiteSpace(x.EntraObjectId))
                .Select(x => x.EntraObjectId)
                .Distinct()
                .ToList();

            if (entraObjectIds.Count == 0)
                return new Dictionary<string, AppUser>();

            var localUsers = await _dbContext.AppUsers
                .AsNoTracking()
                .Where(x => entraObjectIds.Contains(x.EntraObjectId))
                .ToListAsync(cancellationToken);

            return localUsers
                .GroupBy(x => x.EntraObjectId)
                .ToDictionary(x => x.Key, x => x.First());
        }

        private static GraphAppUserDto? ToGraphAppUserDto(GraphUserDto? graphUser, IReadOnlyDictionary<string, AppUser> localUsersByEntraObjectId)
        {
            if (graphUser == null || string.IsNullOrWhiteSpace(graphUser.EntraObjectId))
                return null;

            localUsersByEntraObjectId.TryGetValue(graphUser.EntraObjectId, out var localUser);

            return new GraphAppUserDto
            {
                EntraObjectId = graphUser.EntraObjectId,
                AppUserId = localUser?.Id,
                DisplayName = localUser?.DisplayName ?? graphUser.DisplayName,
                Email = localUser?.Email ?? graphUser.Email,
                UserPrincipalName = graphUser.UserPrincipalName,
                Department = localUser?.Department ?? graphUser.Department,
                JobTitle = localUser?.JobTitle ?? graphUser.JobTitle,
                OfficeLocation = graphUser.OfficeLocation,
                IsKnownLocalUser = localUser != null,
                IsActiveLocalUser = localUser?.IsActive == true
            };
        }

        private async Task SyncHierarchyRelationsAsync(AppUser currentUser, GraphUserHierarchyDto graphHierarchy, IReadOnlyDictionary<string, AppUser> localUsersByEntraObjectId, CancellationToken cancellationToken)
        {
            await SyncUserManagerRelationAsync(
                currentUser,
                graphHierarchy.Manager,
                localUsersByEntraObjectId,
                cancellationToken);

            foreach (var directReport in graphHierarchy.DirectReports)
            {
                if (string.IsNullOrWhiteSpace(directReport.EntraObjectId))
                    continue;

                if (!localUsersByEntraObjectId.TryGetValue(directReport.EntraObjectId, out var localDirectReport))
                    continue;

                await SyncUserManagerRelationAsync(
                    localDirectReport,
                    graphHierarchy.CurrentUser,
                    localUsersByEntraObjectId,
                    cancellationToken);
            }
        }

        private async Task SyncUserManagerRelationAsync(AppUser user, GraphUserDto? manager, IReadOnlyDictionary<string, AppUser> localUsersByEntraObjectId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var managerEntraObjectId = manager?.EntraObjectId;

            AppUser? localManager = null;

            if (!string.IsNullOrWhiteSpace(managerEntraObjectId))
            {
                localUsersByEntraObjectId.TryGetValue(managerEntraObjectId, out localManager);
            }

            var resolvedManagerUserId = localManager?.Id;

            var currentActiveRelation = await _dbContext.AppUserManagerRelations
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.IsActive, cancellationToken);

            if (currentActiveRelation != null && SameNullable(currentActiveRelation.ManagerEntraObjectId, managerEntraObjectId))
            {
                currentActiveRelation.UserEntraObjectId = user.EntraObjectId;
                currentActiveRelation.ManagerUserId = resolvedManagerUserId;
                currentActiveRelation.TenantId = user.TenantId;
                currentActiveRelation.SyncedAtUtc = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            if (currentActiveRelation != null)
            {
                currentActiveRelation.IsActive = false;
                currentActiveRelation.ValidToUtc = now;
                currentActiveRelation.SyncedAtUtc = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var newRelation = new AppUserManagerRelation
            {
                UserId = user.Id,
                UserEntraObjectId = user.EntraObjectId,
                ManagerUserId = resolvedManagerUserId,
                ManagerEntraObjectId = managerEntraObjectId,
                TenantId = user.TenantId,
                SyncedAtUtc = now,
                ValidFromUtc = now,
                ValidToUtc = null,
                IsActive = true
            };

            _dbContext.AppUserManagerRelations.Add(newRelation);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static bool SameNullable(string? left, string? right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static GraphAppUserDto ToGraphAppUserDto(AppUser user)
        {
            return new GraphAppUserDto
            {
                EntraObjectId = user.EntraObjectId,
                AppUserId = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                UserPrincipalName = user.Email,
                Department = user.Department,
                JobTitle = user.JobTitle,
                OfficeLocation = null,
                IsKnownLocalUser = true,
                IsActiveLocalUser = user.IsActive
            };
        }

        private static GraphAppUserDto? ToManagerGraphAppUserDto(AppUserManagerRelation? relation)
        {
            if (relation == null)
                return null;

            if (relation.ManagerUser != null)
                return ToGraphAppUserDto(relation.ManagerUser);

            if (string.IsNullOrWhiteSpace(relation.ManagerEntraObjectId))
                return null;

            return new GraphAppUserDto
            {
                EntraObjectId = relation.ManagerEntraObjectId,
                AppUserId = null,
                DisplayName = null,
                Email = null,
                UserPrincipalName = null,
                Department = null,
                JobTitle = null,
                OfficeLocation = null,
                IsKnownLocalUser = false,
                IsActiveLocalUser = false
            };
        }
    }
}
