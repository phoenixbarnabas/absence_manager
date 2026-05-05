using Data;
using Entities.Dtos.AbsenceRequestDtos;
using Entities.Enums;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Logic.Logic
{
    public class AbsenceRequestLogic
    {
        private readonly AbsenceManagerDbContext _dbContext;

        public AbsenceRequestLogic(AbsenceManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AbsenceRequestViewDto> CreateAsync(
            CreateAbsenceRequestDto dto,
            string currentUserId,
            CancellationToken cancellationToken = default)
        {
            var type = ParseType(dto.Type);
            ValidateDateRange(dto.DateFrom, dto.DateTo);

            var user = await _dbContext.AppUsers
                .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found.");

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active.");

            var hasOverlap = await _dbContext.AbsenceRequests.AnyAsync(x =>
                x.UserId == currentUserId &&
                x.Status != AbsenceRequestStatus.Cancelled &&
                x.Status != AbsenceRequestStatus.Rejected &&
                x.DateFrom <= dto.DateTo &&
                x.DateTo >= dto.DateFrom,
                cancellationToken);

            if (hasOverlap)
                throw new InvalidOperationException("Erre az időszakra már van aktív távollét vagy home office igényed.");

            var request = new AbsenceRequest
            {
                UserId = currentUserId,
                Type = type,
                Status = AbsenceRequestStatus.Pending,
                DateFrom = dto.DateFrom,
                DateTo = dto.DateTo,
                Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByUserId = currentUserId
            };

            _dbContext.AbsenceRequests.Add(request);
            await _dbContext.SaveChangesAsync(cancellationToken);

            request.User = user;

            return ToViewDto(request);
        }

        public async Task<IEnumerable<AbsenceRequestViewDto>> GetMineAsync(
            string currentUserId,
            DateOnly? fromDate,
            DateOnly? toDate,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.AbsenceRequests
                .Include(x => x.User)
                .Where(x => x.UserId == currentUserId);

            if (fromDate.HasValue)
                query = query.Where(x => x.DateTo >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.DateFrom <= toDate.Value);

            var requests = await query
                .OrderByDescending(x => x.DateFrom)
                .ToListAsync(cancellationToken);

            return requests.Select(ToViewDto);
        }

        public async Task<AbsenceRequestViewDto> GetByIdAsync(
            string id,
            string currentUserId,
            CancellationToken cancellationToken = default)
        {
            var currentUser = await _dbContext.AppUsers
                .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found.");

            var request = await _dbContext.AbsenceRequests
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Request not found.");

            if (!CanSeeRequest(currentUser, request))
                throw new UnauthorizedAccessException("You cannot see this request.");

            return ToViewDto(request);
        }

        public async Task CancelAsync(
            string id,
            string currentUserId,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.AbsenceRequests
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Request not found.");

            if (request.UserId != currentUserId)
                throw new UnauthorizedAccessException("You can only cancel your own request.");

            if (request.Status == AbsenceRequestStatus.Cancelled)
                throw new InvalidOperationException("Request is already cancelled.");

            if (request.DateFrom < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Past requests cannot be cancelled.");

            request.Status = AbsenceRequestStatus.Cancelled;
            request.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public static AbsenceRequestType ParseType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Type is required.");

            return value.Trim() switch
            {
                "vacation" => AbsenceRequestType.Vacation,
                "homeOffice" => AbsenceRequestType.HomeOffice,
                "sickLeave" => AbsenceRequestType.SickLeave,
                "otherAbsence" => AbsenceRequestType.OtherAbsence,

                "Vacation" => AbsenceRequestType.Vacation,
                "HomeOffice" => AbsenceRequestType.HomeOffice,
                "SickLeave" => AbsenceRequestType.SickLeave,
                "OtherAbsence" => AbsenceRequestType.OtherAbsence,

                _ => throw new ArgumentException("Unknown absence request type.")
            };
        }

        public static string ToTypeKey(AbsenceRequestType type)
        {
            return type switch
            {
                AbsenceRequestType.Vacation => "vacation",
                AbsenceRequestType.HomeOffice => "homeOffice",
                AbsenceRequestType.SickLeave => "sickLeave",
                AbsenceRequestType.OtherAbsence => "otherAbsence",
                _ => "otherAbsence"
            };
        }

        public static string ToStatusKey(AbsenceRequestStatus status)
        {
            return status switch
            {
                AbsenceRequestStatus.Pending => "pending",
                AbsenceRequestStatus.Approved => "approved",
                AbsenceRequestStatus.Rejected => "rejected",
                AbsenceRequestStatus.Cancelled => "cancelled",
                _ => "pending"
            };
        }

        public static string GetTypeLabel(AbsenceRequestType type)
        {
            return type switch
            {
                AbsenceRequestType.Vacation => "Szabadság",
                AbsenceRequestType.HomeOffice => "Home office",
                AbsenceRequestType.SickLeave => "Betegszabadság",
                AbsenceRequestType.OtherAbsence => "Egyéb távollét",
                _ => "Távollét"
            };
        }

        public static AbsenceRequestViewDto ToViewDto(AbsenceRequest request)
        {
            return new AbsenceRequestViewDto
            {
                Id = request.Id,
                Type = ToTypeKey(request.Type),
                Status = ToStatusKey(request.Status),
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                Reason = request.Reason,
                UserId = request.UserId,
                UserName = request.User.DisplayName,
                Department = request.User.Department,
                CreatedAtUtc = request.CreatedAtUtc
            };
        }

        private static void ValidateDateRange(DateOnly fromDate, DateOnly toDate)
        {
            if (toDate < fromDate)
                throw new ArgumentException("A záró dátum nem lehet korábbi, mint a kezdő dátum.");

            if (fromDate < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Múltbeli napra nem lehet igényt leadni.");

            if (toDate.DayNumber - fromDate.DayNumber > 60)
                throw new InvalidOperationException("Egy igény legfeljebb 60 napos időszakra adható le.");
        }

        private static bool CanSeeRequest(AppUser currentUser, AbsenceRequest request)
        {
            if (request.UserId == currentUser.Id)
                return true;

            if (!string.IsNullOrWhiteSpace(currentUser.Department) &&
                currentUser.Department == request.User.Department)
                return true;

            return IsHrOrAdmin(currentUser);
        }

        public static bool IsHrOrAdmin(AppUser user)
        {
            var title = user.JobTitle ?? string.Empty;

            return title.Contains("hr", StringComparison.OrdinalIgnoreCase)
                || title.Contains("admin", StringComparison.OrdinalIgnoreCase)
                || title.Contains("administrator", StringComparison.OrdinalIgnoreCase)
                || title.Contains("people", StringComparison.OrdinalIgnoreCase);
        }
    }
}