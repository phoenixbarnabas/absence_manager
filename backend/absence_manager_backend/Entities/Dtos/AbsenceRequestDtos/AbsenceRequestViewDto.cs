using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.AbsenceRequestDtos
{
    public class AbsenceRequestViewDto
    {
        public string Id { get; set; } = null!;

        public string Type { get; set; } = null!;

        public string Status { get; set; } = null!;

        public DateOnly DateFrom { get; set; }

        public DateOnly DateTo { get; set; }

        public string? Reason { get; set; }

        public string UserId { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string? Department { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
