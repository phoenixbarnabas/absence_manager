using Data;
using Entities.Dtos.AppUserDtos;
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
            return new UserProfileDto
            {
                Id = userId,
                DisplayName = "Nagy Péter",
                Email = "nagy.peter@phoenixhu",
                Department = "IT",
                JobTitle = "Vezetői"
            };
        }
    }
}
