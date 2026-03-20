using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Office
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        public Location Location { get; set; } = null!;
        public ICollection<Workstation> Workstations { get; set; } = new List<Workstation>();
    }
}
