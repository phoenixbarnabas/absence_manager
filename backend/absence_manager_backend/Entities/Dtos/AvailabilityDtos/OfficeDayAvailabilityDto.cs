using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.Availability
{
    public class OfficeDayAvailabilityDto
    {
        public DateOnly BookingDate { get; set; }

        public int LocationId { get; set; }
        public string LocationName { get; set; } = null!;

        public int OfficeId { get; set; }
        public string OfficeName { get; set; } = null!;

        public int TotalWorkstations { get; set; }
        public int BookedWorkstations { get; set; }
        public int FreeWorkstations { get; set; }

        public bool CurrentUserHasBooking { get; set; }
        public int? CurrentUserBookingId { get; set; }
        public int? CurrentUserWorkstationId { get; set; }

        public List<WorkstationAvailabilityDto> Workstations { get; set; } = new();
    }
}
