using Data;
using Entities.Dtos.AppUserDtos;
using Entities.Dtos.WorkStationDtos;
using Entities.Models;
using Logic.Helper;
using Microsoft.EntityFrameworkCore;

namespace Logic.Logic
{
    public class UserLogic
    {
        private readonly Repository<AppUser> _userRepository;
        private readonly DtoProvider _dtoProvider;
        private readonly AbsenceManagerDbContext _dbContext;
        private readonly IMsGraphLogic _graphLogic;

        public UserLogic(
            Repository<AppUser> userRepository,
            DtoProvider dtoProvider,
            AbsenceManagerDbContext dbContext,
            IMsGraphLogic graphLogic)
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

        public async Task RefreshUserProfileAsync(string userId, CancellationToken ct)
        {
            var user = await _dbContext.AppUsers.FindAsync(userId);

            var graphProfile = await _graphLogic.GetCurrentUserProfileAsync(ct);

            if (graphProfile == null) return;

            var changed = false;

            if (user.DisplayName != graphProfile.DisplayName)
            {
                user.DisplayName = graphProfile.DisplayName;
                changed = true;
            }

            if (user.Email != graphProfile.Email)
            {
                user.Email = graphProfile.Email;
                changed = true;
            }

            if (user.Department != graphProfile.Department)
            {
                user.Department = graphProfile.Department;
                changed = true;
            }

            if (user.JobTitle != graphProfile.JobTitle)
            {
                user.JobTitle = graphProfile.JobTitle;
                changed = true;
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(ct);
            }
        }
    }
}
