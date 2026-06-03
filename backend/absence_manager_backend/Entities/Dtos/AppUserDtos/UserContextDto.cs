using Entities.Dtos.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.AppUserDtos
{
    public class UserContextDto
    {
        public UserProfileDto Profile { get; set; } = null!;

        public AppUserHierarchyDto Hierarchy { get; set; } = new();

        public bool IsManager { get; set; }

        public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

        public DateTime? LastGraphSyncAtUtc { get; set; }
    }
}
