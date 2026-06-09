using Entities.Enums;
using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities.Models
{
    public class EmailLog : IIdEntity
    {
        public EmailLog()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; } = null!;

        public string NotificationType { get; set; } = null!;

        public EmailLogStatus Status { get; set; } = EmailLogStatus.Pending;

        public string RecipientEmail { get; set; } = null!;

        public string? RecipientName { get; set; }

        public string? SenderEmail { get; set; }

        public string Subject { get; set; } = null!;

        public string BodyHtml { get; set; } = null!;

        public string? ErrorMessage { get; set; }

        public string? Provider { get; set; }

        public int AttemptCount { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? SentAtUtc { get; set; }

        public DateTime? LastAttemptAtUtc { get; set; }

        public string? AbsenceRequestId { get; set; }

        public AbsenceRequest? AbsenceRequest { get; set; }
    }
}
