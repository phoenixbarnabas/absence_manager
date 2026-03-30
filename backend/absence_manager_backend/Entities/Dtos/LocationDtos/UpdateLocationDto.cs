using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.LocationDtos
{
    public class UpdateLocationDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
