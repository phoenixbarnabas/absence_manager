using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Workstation : IIdEntity
    {
        public Workstation() 
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        public string OfficeId { get; set; }

        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        public decimal? PositionX { get; set; }
        public decimal? PositionY { get; set; }

        public Office Office { get; set; } = null!;
        public ICollection<OfficeBooking> Bookings { get; set; } = new List<OfficeBooking>();
    }
}
