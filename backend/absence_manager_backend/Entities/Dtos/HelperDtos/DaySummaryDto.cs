using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.Helpers
{
    public class DaySummaryDto
    {
        public DateOnly Date { get; set; }
        public int TotalWorkstations { get; set; }
        public int BookedWorkstations { get; set; }
        public int FreeWorkstations { get; set; }
        public bool CurrentUserHasBooking { get; set; }
    }
}
