namespace Entities.Dtos.AppUserDtos
{
    public class UserProfileDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? Email { get; set; }
        public string Department { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
    }
}
