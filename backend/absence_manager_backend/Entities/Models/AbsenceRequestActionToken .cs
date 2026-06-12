using Entities.Enums;
using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities.Models
{
    public class AbsenceRequestActionToken : IIdEntity
    {
        public AbsenceRequestActionToken()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; } = null!;

        public string AbsenceRequestId { get; set; } = null!;

        public string ManagerUserId { get; set; } = null!;

        public AbsenceRequestEmailAction Action { get; set; }

        public string TokenHash { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? UsedAtUtc { get; set; }

        public bool IsUsed { get; set; }

        public AbsenceRequest AbsenceRequest { get; set; } = null!;

        public AppUser ManagerUser { get; set; } = null!;
    }
}
