using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.AbsenceRequestDtos
{
    public class AbsenceRequestApprovalDto
    {
        public string Id { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string? UserDisplayName { get; set; }

        public string? UserEmail { get; set; }

        public AbsenceRequestType Type { get; set; }

        public AbsenceRequestStatus Status { get; set; }

        public DateOnly DateFrom { get; set; }

        public DateOnly DateTo { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }

        public string? ReviewedByUserId { get; set; }

        public string? ReviewedByUserName { get; set; }

        public string? DecisionComment { get; set; }
    }
}
