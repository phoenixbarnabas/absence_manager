using Data;
using Entities.Dtos.CalendarDtos;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Logic.Logic
{
    public class CalendarLogic
    {
        private const int MaximumRangeInDays = 370;

        private readonly Repository<OfficeBooking> _officeBookingRepository;
        private readonly Repository<AppUser> _appUserRepository;

        public CalendarLogic(
            Repository<OfficeBooking> officeBookingRepository,
            Repository<AppUser> appUserRepository)
        {
            _officeBookingRepository = officeBookingRepository;
            _appUserRepository = appUserRepository;
        }

        public IEnumerable<CalendarDayInfoDto> GetDayInfos(DateOnly fromDate, DateOnly toDate)
        {
            ValidateDateRange(fromDate, toDate);

            var result = new List<CalendarDayInfoDto>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                result.Add(CreateDayInfo(date));
            }

            return result;
        }

        public IEnumerable<CalendarEventDto> GetEvents(
            DateOnly fromDate,
            DateOnly toDate,
            string currentUserId,
            string scope,
            IEnumerable<string>? eventTypes)
        {
            ValidateDateRange(fromDate, toDate);

            var requestedTypes = eventTypes?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (requestedTypes.Count > 0 && !requestedTypes.Contains("deskBooking"))
                return Enumerable.Empty<CalendarEventDto>();

            var currentUser = _appUserRepository.FindById(currentUserId);

            if (!currentUser.IsActive)
                throw new InvalidOperationException("User is not active.");

            var normalizedScope = string.IsNullOrWhiteSpace(scope)
                ? "mine"
                : scope.Trim().ToLowerInvariant();

            var query = _officeBookingRepository.GetAll()
                .Include(b => b.User)
                .Include(b => b.Workstation)
                .ThenInclude(w => w.Office)
                .ThenInclude(o => o.Location)
                .Where(b =>
                    b.BookingDate >= fromDate &&
                    b.BookingDate <= toDate &&
                    !b.IsCancelled);

            query = normalizedScope switch
            {
                "mine" => query.Where(b => b.UserId == currentUserId),
                "team" => ApplyTeamVisibility(query, currentUser),
                "organization" => ApplyOrganizationVisibility(query, currentUser),
                _ => throw new ArgumentException("Unknown calendar scope.")
            };

            return query
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.User.DisplayName)
                .ToList()
                .Select(ToDeskBookingCalendarEvent);
        }

        private IQueryable<OfficeBooking> ApplyTeamVisibility(IQueryable<OfficeBooking> query, AppUser currentUser)
        {
            if (!CanSeeTeamCalendar(currentUser))
                throw new UnauthorizedAccessException("Team calendar view is not allowed for the current user.");

            if (string.IsNullOrWhiteSpace(currentUser.Department))
                return query.Where(b => b.UserId == currentUser.Id);

            return query.Where(b => b.User.Department == currentUser.Department);
        }

        private IQueryable<OfficeBooking> ApplyOrganizationVisibility(IQueryable<OfficeBooking> query, AppUser currentUser)
        {
            if (!CanSeeOrganizationCalendar(currentUser))
                throw new UnauthorizedAccessException("Organization calendar view is not allowed for the current user.");

            return query;
        }

        private static CalendarEventDto ToDeskBookingCalendarEvent(OfficeBooking booking)
        {
            return new CalendarEventDto
            {
                Id = $"desk-booking-{booking.Id}",
                SourceId = booking.Id,
                Title = $"Helyfoglalás - {booking.Workstation.Office.Name} / {booking.Workstation.Name}",
                Type = "deskBooking",
                Status = "approved",
                DateFrom = booking.BookingDate,
                DateTo = booking.BookingDate,
                UserId = booking.UserId,
                UserName = booking.User.DisplayName,
                Description = "Irodai munkaállomás foglalás.",
                DetailsUrl = "/desk-booking",
                LocationName = booking.Workstation.Office.Location.Name,
                OfficeName = booking.Workstation.Office.Name,
                WorkstationName = booking.Workstation.Name
            };
        }

        private static bool CanSeeTeamCalendar(AppUser user)
        {
            return CanSeeOrganizationCalendar(user)
                || ContainsAny(user.JobTitle, "manager", "lead", "vezető", "vezeto", "team lead");
        }

        private static bool CanSeeOrganizationCalendar(AppUser user)
        {
            return ContainsAny(user.JobTitle, "hr", "admin", "administrator", "people");
        }

        private static bool ContainsAny(string? value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        private static CalendarDayInfoDto CreateDayInfo(DateOnly date)
        {
            var holidays = GetHungarianPublicHolidays(date.Year);
            holidays.TryGetValue(date, out var holidayName);

            var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var isHoliday = !string.IsNullOrWhiteSpace(holidayName);

            return new CalendarDayInfoDto
            {
                Date = date,
                IsWeekend = isWeekend,
                IsHoliday = isHoliday,
                IsWorkingDay = !isWeekend && !isHoliday,
                HolidayName = holidayName
            };
        }

        private static Dictionary<DateOnly, string> GetHungarianPublicHolidays(int year)
        {
            var easterSunday = GetEasterSunday(year);

            return new Dictionary<DateOnly, string>
            {
                [new DateOnly(year, 1, 1)] = "Újév",
                [new DateOnly(year, 3, 15)] = "Nemzeti ünnep",
                [easterSunday.AddDays(-2)] = "Nagypéntek",
                [easterSunday.AddDays(1)] = "Húsvéthétfő",
                [new DateOnly(year, 5, 1)] = "Munka ünnepe",
                [easterSunday.AddDays(50)] = "Pünkösdhétfő",
                [new DateOnly(year, 8, 20)] = "Államalapítás ünnepe",
                [new DateOnly(year, 10, 23)] = "Nemzeti ünnep",
                [new DateOnly(year, 11, 1)] = "Mindenszentek",
                [new DateOnly(year, 12, 25)] = "Karácsony",
                [new DateOnly(year, 12, 26)] = "Karácsony másnapja"
            };
        }

        private static DateOnly GetEasterSunday(int year)
        {
            var a = year % 19;
            var b = year / 100;
            var c = year % 100;
            var d = b / 4;
            var e = b % 4;
            var f = (b + 8) / 25;
            var g = (b - f + 1) / 3;
            var h = (19 * a + b - d - g + 15) % 30;
            var i = c / 4;
            var k = c % 4;
            var l = (32 + 2 * e + 2 * i - h - k) % 7;
            var m = (a + 11 * h + 22 * l) / 451;
            var month = (h + l - 7 * m + 114) / 31;
            var day = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateOnly(year, month, day);
        }

        private static void ValidateDateRange(DateOnly fromDate, DateOnly toDate)
        {
            if (toDate < fromDate)
                throw new ArgumentException("toDate cannot be earlier than fromDate.");

            if (toDate.DayNumber - fromDate.DayNumber > MaximumRangeInDays)
                throw new ArgumentException($"The selected range cannot be longer than {MaximumRangeInDays} days.");
        }
    }
}
