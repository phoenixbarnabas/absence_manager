using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.Graph
{
    public class GraphAppUserDto
    {
        public string EntraObjectId { get; set; } = null!;

        public string? AppUserId { get; set; }

        public string? DisplayName { get; set; }

        public string? Email { get; set; }

        public string? UserPrincipalName { get; set; }

        public string? Department { get; set; }

        public string? JobTitle { get; set; }

        public string? OfficeLocation { get; set; }

        public bool IsKnownLocalUser { get; set; }

        public bool IsActiveLocalUser { get; set; }
    }
}
