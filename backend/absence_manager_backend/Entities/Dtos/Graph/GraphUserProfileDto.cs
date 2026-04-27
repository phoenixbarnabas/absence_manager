using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.Graph
{
    public class GraphUserProfileDto
    {
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string? OfficeLocation { get; set; }
        public string? PreferredLanguage { get; set; }
    }
}
