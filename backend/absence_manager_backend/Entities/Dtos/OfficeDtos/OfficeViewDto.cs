using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.OfficeDtos
{
    public class OfficeViewDto
    {
        public string Id { get; set; }
        public string LocationId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
