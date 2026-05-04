using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.CalendarDtos
{
    public class CalendarEventDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Description { get; set; }
        public string? SourceId { get; set; }
        public string? DetailsUrl { get; set; }
        public string? LocationName { get; set; }
        public string? OfficeName { get; set; }
        public string? WorkstationName { get; set; }
    }
}
