using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Location : IIdEntity
    {
        public Location() 
        { 
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        public ICollection<Office> Offices { get; set; } = new List<Office>();
    }
}
