using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.Availability
{
    public class WorkstationAvailabilityDto
    {
        public string WorkstationId { get; set; }
        public string WorkstationCode { get; set; } = null!;
        public string WorkstationName { get; set; } = null!;
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
        public bool IsBooked { get; set; }
        public bool IsBookedByCurrentUser { get; set; }

        public string? BookedByUserId { get; set; }
        public string? BookedByDisplayName { get; set; }

        public decimal? PositionX { get; set; }
        public decimal? PositionY { get; set; }
        public string? BookingId { get; set; }
        public string? BookedByUserName { get; set; }
        public bool CanCurrentUserBook { get; set; }
    }
}
