using Entities.Helpers;
using System;
using System.Collections.Generic;
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
        public string Id { get; set; }

        public string WorkstationId { get; set; }
        public string AppUserId { get; set; } = null!;

        public DateOnly BookingDate { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByUserId { get; set; } = null!;

        public DateTime? CancelledAtUtc { get; set; }
        public string? CancelledByUserId { get; set; }
        public bool IsCancelled { get; set; }

        public Workstation Workstation { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;
    }
}
