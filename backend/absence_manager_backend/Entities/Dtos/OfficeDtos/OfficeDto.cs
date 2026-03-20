using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.OfficeDtos
{
    public class OfficeDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
