using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities.Models
{
    public class AppUserManagerRelation : IIdEntity
    {
        public AppUserManagerRelation()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string UserEntraObjectId { get; set; } = null!;

        public string? ManagerUserId { get; set; }

        public string? ManagerEntraObjectId { get; set; }

        public string? TenantId { get; set; }

        public DateTime SyncedAtUtc { get; set; }

        public DateTime ValidFromUtc { get; set; }

        public DateTime? ValidToUtc { get; set; }

        public bool IsActive { get; set; }

        public AppUser User { get; set; } = null!;

        public AppUser? ManagerUser { get; set; }
    }
}
