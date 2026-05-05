using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.CalendarDtos
{
    public class CalendarDayInfoDto
    {
        public DateOnly Date { get; set; }

        public bool IsWeekend { get; set; }

        public bool IsHoliday { get; set; }

        public bool IsWorkingDay { get; set; }

        public string? HolidayName { get; set; }
    }
}
