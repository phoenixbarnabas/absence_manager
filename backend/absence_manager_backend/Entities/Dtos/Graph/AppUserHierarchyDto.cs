using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.Graph
{
    public class AppUserHierarchyDto
    {
        public GraphAppUserDto? CurrentUser { get; set; }

        public GraphAppUserDto? Manager { get; set; }

        public IReadOnlyList<GraphAppUserDto> DirectReports { get; set; } = Array.Empty<GraphAppUserDto>();
    }
}
