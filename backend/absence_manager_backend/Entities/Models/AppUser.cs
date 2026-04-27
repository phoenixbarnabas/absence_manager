using Entities.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class AppUser : IIdEntity
    {
        [Key]
        public string Id { get; set; }
        public string EntraObjectId { get; set; }
        public string? TenantId { get; set; }
        public string DisplayName { get; set; }
        public string? Email { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<OfficeBooking> OfficeBookings { get; set; } = new List<OfficeBooking>();

        public AppUser()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
