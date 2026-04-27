using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class OfficeBooking : IIdEntity
    {
        public OfficeBooking()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }

        public string WorkstationId { get; set; }
        public string UserId { get; set; } = null!;

        public DateOnly BookingDate { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByUserId { get; set; } = null!;

        public DateTime? CancelledAtUtc { get; set; }
        public string? CancelledByUserId { get; set; }
        public bool IsCancelled { get; set; }

        public Workstation Workstation { get; set; } = null!;
        public AppUser User { get; set; } = null!;
    }
}
