using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models
{
    public class UserActivityLog : IIdEntity
    {
        public UserActivityLog()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public string? ActorUserId { get; set; }

        public string? ActorEntraObjectId { get; set; }

        public string? TenantId { get; set; }

        public string Action { get; set; } = null!;

        public string EntityType { get; set; } = null!;

        public string? EntityId { get; set; }

        public string Outcome { get; set; } = "Success";

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? RequestMethod { get; set; }

        public string? RequestPath { get; set; }

        public string? CorrelationId { get; set; }

        public string? MetadataJson { get; set; }
    }
}
