using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.Graph
{
    public class GraphUserHierarchyDto
    {
        public GraphUserDto? CurrentUser { get; set; }

        public GraphUserDto? Manager { get; set; }

        public IReadOnlyList<GraphUserDto> DirectReports { get; set; } = Array.Empty<GraphUserDto>();
    }
}
