using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.UserActivityLogDtos
{
    public class UserActivityLogViewDto
    {
        public string Id { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public string? ActorUserId { get; set; }

        public string? ActorDisplayName { get; set; }

        public string? ActorEmail { get; set; }

        public string? ActorEntraObjectId { get; set; }

        public string? TenantId { get; set; }

        public string Action { get; set; } = null!;

        public string EntityType { get; set; } = null!;

        public string? EntityId { get; set; }

        public string Outcome { get; set; } = null!;

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? RequestMethod { get; set; }

        public string? RequestPath { get; set; }

        public string? CorrelationId { get; set; }

        public string? MetadataJson { get; set; }
    }
}
