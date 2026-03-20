using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.OfficeBooking
{
    public class CreateOfficeBookingDto
    {
        public int WorkstationId { get; set; }
        public DateOnly BookingDate { get; set; }
    }
}
