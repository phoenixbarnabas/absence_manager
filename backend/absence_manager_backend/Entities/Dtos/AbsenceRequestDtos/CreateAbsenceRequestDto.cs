using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.AbsenceRequestDtos
{
    public class CreateAbsenceRequestDto
    {
        public string Type { get; set; } = null!;

        public DateOnly DateFrom { get; set; }

        public DateOnly DateTo { get; set; }

        public string? Reason { get; set; }
    }
}
