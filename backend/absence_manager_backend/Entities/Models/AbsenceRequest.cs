using Entities.Enums;
using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities.Models
{
    public class AbsenceRequest : IIdEntity
    {
        public AbsenceRequest()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public AbsenceRequestType Type { get; set; }

        public AbsenceRequestStatus Status { get; set; }

        public DateOnly DateFrom { get; set; }

        public DateOnly DateTo { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public string CreatedByUserId { get; set; } = null!;

        public DateTime? UpdatedAtUtc { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }

        public string? ReviewedByUserId { get; set; }

        public AppUser User { get; set; } = null!;

        public AppUser? ReviewedByUser { get; set; }
    }
}
