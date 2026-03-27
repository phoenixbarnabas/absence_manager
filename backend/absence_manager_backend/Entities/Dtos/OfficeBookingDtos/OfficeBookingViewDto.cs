using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.OfficeBooking
{
    public class OfficeBookingViewDto
    {
        public string Id { get; set; }
        public DateOnly BookingDate { get; set; }

        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;

        public string WorkstationId { get; set; }
        public string WorkstationCode { get; set; } = null!;
        public string WorkstationName { get; set; } = null!;

        public string OfficeId { get; set; }
        public string OfficeName { get; set; } = null!;

        public string LocationId { get; set; }
        public string LocationName { get; set; } = null!;

        public bool IsCancelled { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
