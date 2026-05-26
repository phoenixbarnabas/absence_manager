using Data;
using Entities.Dtos.CalendarDtos;
using Entities.Enums;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Logic.Logic
{
    public class CalendarLogic
    {
        private const int MaximumRangeInDays = 370;

        private readonly AbsenceManagerDbContext _dbContext;

        public CalendarLogic(AbsenceManagerDbContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<IEnumerable<CalendarEventDto>> GetEventsAsync(
            DateOnly fromDate,
            DateOnly toDate,
            string currentUserId,
            string scope,
            IEnumerable<string>? eventTypes,
            CancellationToken cancellationToken = default)
        {
            ValidateDateRange(fromDate, toDate);

            var requestedTypes = NormalizeRequestedTypes(eventTypes);

            var currentUser = await _dbContext.AppUsers
                .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found.");

            if (!currentUser.IsActive)
                throw new InvalidOperationException("User is not active.");

            var normalizedScope = string.IsNullOrWhiteSpace(scope)
                ? "mine"
                : scope.Trim().ToLowerInvariant();

            var result = new List<CalendarEventDto>();

            if (requestedTypes.Count == 0 || requestedTypes.Any(IsAbsenceType))
            {
                var absenceEvents = await GetAbsenceEventsAsync(
                    fromDate,
                    toDate,
                    currentUser,
                    normalizedScope,
                    requestedTypes,
                    cancellationToken);

                result.AddRange(absenceEvents);
            }

            if (requestedTypes.Count == 0 || requestedTypes.Contains("deskBooking"))
            {
                var deskBookingEvents = await GetDeskBookingEventsAsync(
                    fromDate,
                    toDate,
                    currentUser,
                    normalizedScope,
                    cancellationToken);

                result.AddRange(deskBookingEvents);
            }

            return result
                .OrderBy(x => x.DateFrom)
                .ThenBy(x => x.UserName)
                .ThenBy(x => x.Title)
                .ToList();
        }

        private async Task<IEnumerable<CalendarEventDto>> GetAbsenceEventsAsync(
            DateOnly fromDate,
            DateOnly toDate,
            AppUser currentUser,
            string scope,
            HashSet<string> requestedTypes,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.AbsenceRequests
                .Include(x => x.User)
                .Where(x =>
                    x.DateFrom <= toDate &&
                    x.DateTo >= fromDate &&
                    x.Status != AbsenceRequestStatus.Cancelled);

            query = ApplyAbsenceVisibility(query, currentUser, scope);

            if (requestedTypes.Count > 0)
            {
                var requestedAbsenceTypes = requestedTypes
                    .Where(IsAbsenceType)
                    .Select(AbsenceRequestLogic.ParseType)
                    .ToHashSet();

                query = query.Where(x => requestedAbsenceTypes.Contains(x.Type));
            }

            var requests = await query
                .OrderBy(x => x.DateFrom)
                .ThenBy(x => x.User.DisplayName)
                .ToListAsync(cancellationToken);

            return requests.Select(ToAbsenceCalendarEvent);
        }

        private async Task<IEnumerable<CalendarEventDto>> GetDeskBookingEventsAsync(
            DateOnly fromDate,
            DateOnly toDate,
            AppUser currentUser,
            string scope,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.OfficeBookings
                .Include(b => b.User)
                .Include(b => b.Workstation)
                .ThenInclude(w => w.Office)
                .ThenInclude(o => o.Location)
                .Where(b =>
                    b.BookingDate >= fromDate &&
                    b.BookingDate <= toDate &&
                    !b.IsCancelled);

            query = ApplyDeskBookingVisibility(query, currentUser, scope);

            var bookings = await query
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.User.DisplayName)
                .ToListAsync(cancellationToken);

            return bookings.Select(ToDeskBookingCalendarEvent);
        }

        private static IQueryable<AbsenceRequest> ApplyAbsenceVisibility(
            IQueryable<AbsenceRequest> query,
            AppUser currentUser,
            string scope)
        {
            return scope switch
            {
                "mine" => query.Where(x => x.UserId == currentUser.Id),

                "team" => string.IsNullOrWhiteSpace(currentUser.Department)
                    ? query.Where(x => x.UserId == currentUser.Id)
                    : query.Where(x => x.User.Department == currentUser.Department),

                "organization" => AbsenceRequestLogic.IsHrOrAdmin(currentUser)
                    ? query
                    : query.Where(x => x.UserId == currentUser.Id),

                _ => query.Where(x => x.UserId == currentUser.Id)
            };
        }

        private static IQueryable<OfficeBooking> ApplyDeskBookingVisibility(
            IQueryable<OfficeBooking> query,
            AppUser currentUser,
            string scope)
        {
            return scope switch
            {
                "mine" => query.Where(x => x.UserId == currentUser.Id),

                "team" => string.IsNullOrWhiteSpace(currentUser.Department)
                    ? query.Where(x => x.UserId == currentUser.Id)
                    : query.Where(x => x.User.Department == currentUser.Department),

                "organization" => AbsenceRequestLogic.IsHrOrAdmin(currentUser)
                    ? query
                    : query.Where(x => x.UserId == currentUser.Id),

                _ => query.Where(x => x.UserId == currentUser.Id)
            };
        }

        private static CalendarEventDto ToAbsenceCalendarEvent(AbsenceRequest request)
        {
            var typeKey = AbsenceRequestLogic.ToTypeKey(request.Type);
            var typeLabel = AbsenceRequestLogic.GetTypeLabel(request.Type);

            return new CalendarEventDto
            {
                Id = $"absence-{request.Id}",
                SourceId = request.Id,
                Title = $"{typeLabel} - {request.User.DisplayName}",
                Type = typeKey,
                Status = AbsenceRequestLogic.ToStatusKey(request.Status),
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                UserId = request.UserId,
                UserName = request.User.DisplayName,
                Department = request.User.Department,
                Description = request.Reason,
                DetailsUrl = $"/calendar?requestId={request.Id}"
            };
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
                Department = booking.User.Department,
                Description = "Irodai munkaállomás foglalás.",
                DetailsUrl = "/desk-booking",
                LocationName = booking.Workstation.Office.Location.Name,
                OfficeName = booking.Workstation.Office.Name,
                WorkstationName = booking.Workstation.Name
            };
        }

        private static HashSet<string> NormalizeRequestedTypes(IEnumerable<string>? eventTypes)
        {
            return eventTypes?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsAbsenceType(string type)
        {
            return type.Equals("vacation", StringComparison.OrdinalIgnoreCase)
                || type.Equals("homeOffice", StringComparison.OrdinalIgnoreCase)
                || type.Equals("sickLeave", StringComparison.OrdinalIgnoreCase)
                || type.Equals("otherAbsence", StringComparison.OrdinalIgnoreCase);
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