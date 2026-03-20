using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.LocationDtos
{
    public class CreateLocationDto
    {
        public string Name { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
