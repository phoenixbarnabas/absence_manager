using Data;
using Entities.Dtos.AppUserDtos;
using Entities.Dtos.WorkStationDtos;
using Entities.Models;
using Logic.Helper;

namespace Logic.Logic
{
    public class UserLogic
    {
        private readonly Repository<AppUser> _userRepository;
        private readonly DtoProvider _dtoProvider;

        public UserLogic(
            Repository<AppUser> userRepository,
            DtoProvider dtoProvider)
        {
            _userRepository = userRepository;
            _dtoProvider = dtoProvider;
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
    }
}
