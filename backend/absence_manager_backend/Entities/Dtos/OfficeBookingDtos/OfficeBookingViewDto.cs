using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.OfficeBooking
{
    public class OfficeBookingViewDto
    {
        public int Id { get; set; }
        public DateOnly BookingDate { get; set; }

        public string AppUserId { get; set; } = null!;
        public string AppUserName { get; set; } = null!;

        public int WorkstationId { get; set; }
        public string WorkstationCode { get; set; } = null!;
        public string WorkstationName { get; set; } = null!;

        public int OfficeId { get; set; }
        public string OfficeName { get; set; } = null!;

        public int LocationId { get; set; }
        public string LocationName { get; set; } = null!;

        public bool IsCancelled { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
