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

        public async Task<IReadOnlyList<AbsenceRequestApprovalDto>> GetReviewedApprovalsForManagerAsync(string managerUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(managerUserId))
            {
                throw new ArgumentException("Manager user id is required.", nameof(managerUserId));
            }

            var hasDirectReports = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .AnyAsync(x => x.ManagerUserId == managerUserId && x.IsActive, cancellationToken);

            if (!hasDirectReports)
            {
                return [];
            }

            return await _dbContext.AbsenceRequests
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.ReviewedByUser)
                .Where(x =>
                    x.ReviewedByUserId == managerUserId &&
                    (
                        x.Status == AbsenceRequestStatus.Approved ||
                        x.Status == AbsenceRequestStatus.Rejected
                    ))
                .OrderByDescending(x => x.ReviewedAtUtc)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Select(x => new AbsenceRequestApprovalDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserDisplayName = x.User.DisplayName,
                    UserEmail = x.User.Email,
                    Type = x.Type,
                    Status = x.Status,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    Reason = x.Reason,
                    CreatedAtUtc = x.CreatedAtUtc,
                    ReviewedAtUtc = x.ReviewedAtUtc,
                    ReviewedByUserId = x.ReviewedByUserId,
                    ReviewedByUserName = x.ReviewedByUser != null
                        ? x.ReviewedByUser.DisplayName
                        : null,
                    DecisionComment = x.DecisionComment
                })
                .ToListAsync(cancellationToken);
        }

        public async Task ApproveAbsenceRequestAsync(string absenceRequestId, string managerUserId, string? decisionComment, CancellationToken cancellationToken = default)
        {
            await ReviewAbsenceRequestAsync(
                absenceRequestId,
                managerUserId,
                AbsenceRequestStatus.Approved,
                decisionComment,
                cancellationToken);
        }

        public async Task RejectAbsenceRequestAsync(string absenceRequestId, string managerUserId, string? decisionComment, CancellationToken cancellationToken = default)
        {
            await ReviewAbsenceRequestAsync(
                absenceRequestId,
                managerUserId,
                AbsenceRequestStatus.Rejected,
                decisionComment,
                cancellationToken);
        }

        public async Task<IReadOnlyList<AbsenceRequestApprovalDto>> GetPendingApprovalsForManagerAsync(string managerUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(managerUserId))
            {
                throw new ArgumentException("Manager user id is required.", nameof(managerUserId));
            }

            var directReportUserIds = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .Where(x => x.ManagerUserId == managerUserId && x.IsActive)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (directReportUserIds.Count == 0)
            {
                return [];
            }

            return await _dbContext.AbsenceRequests
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    directReportUserIds.Contains(x.UserId) &&
                    x.Status == AbsenceRequestStatus.Pending)
                .OrderBy(x => x.DateFrom)
                .ThenBy(x => x.CreatedAtUtc)
                .Select(x => new AbsenceRequestApprovalDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserDisplayName = x.User.DisplayName,
                    UserEmail = x.User.Email,
                    Type = x.Type,
                    Status = x.Status,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    Reason = x.Reason,
                    CreatedAtUtc = x.CreatedAtUtc,
                    DecisionComment = x.DecisionComment
                })
                .ToListAsync(cancellationToken);
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

        public async Task CancelAsync(string id, string currentUserId, CancellationToken cancellationToken = default)
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

        private static AbsenceRequestViewDto ToViewDto(AbsenceRequest request)
        {
            return new AbsenceRequestViewDto
            {
                Id = request.Id,
                Type = request.Type.ToString(),
                Status = request.Status.ToString(),
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                Reason = request.Reason,
                UserId = request.UserId,
                UserName = request.User.DisplayName,
                Department = request.User.Department,
                CreatedAtUtc = request.CreatedAtUtc,

                UpdatedAtUtc = request.UpdatedAtUtc,
                ReviewedAtUtc = request.ReviewedAtUtc,
                ReviewedByUserId = request.ReviewedByUserId,
                ReviewedByUserName = request.ReviewedByUser?.DisplayName,
                DecisionComment = request.DecisionComment
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

        private async Task ReviewAbsenceRequestAsync(string absenceRequestId, string managerUserId, AbsenceRequestStatus targetStatus, string? decisionComment, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(absenceRequestId))
            {
                throw new ArgumentException("Absence request id is required.", nameof(absenceRequestId));
            }

            if (string.IsNullOrWhiteSpace(managerUserId))
            {
                throw new ArgumentException("Manager user id is required.", nameof(managerUserId));
            }

            if (targetStatus != AbsenceRequestStatus.Approved &&
                targetStatus != AbsenceRequestStatus.Rejected)
            {
                throw new ArgumentException("Target status must be Approved or Rejected.", nameof(targetStatus));
            }

            var absenceRequest = await _dbContext.AbsenceRequests
                .FirstOrDefaultAsync(x => x.Id == absenceRequestId, cancellationToken);

            if (absenceRequest == null)
            {
                throw new KeyNotFoundException("Absence request was not found.");
            }

            if (absenceRequest.Status != AbsenceRequestStatus.Pending)
            {
                throw new InvalidOperationException("Only pending absence requests can be reviewed.");
            }

            var isActiveManager = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == absenceRequest.UserId &&
                    x.ManagerUserId == managerUserId &&
                    x.IsActive,
                    cancellationToken);

            if (!isActiveManager)
            {
                throw new UnauthorizedAccessException("You are not allowed to review this absence request.");
            }

            var now = DateTime.UtcNow;

            absenceRequest.Status = targetStatus;
            absenceRequest.ReviewedByUserId = managerUserId;
            absenceRequest.ReviewedAtUtc = now;
            absenceRequest.UpdatedAtUtc = now;
            absenceRequest.DecisionComment = string.IsNullOrWhiteSpace(decisionComment)
                ? null
                : decisionComment.Trim();

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}