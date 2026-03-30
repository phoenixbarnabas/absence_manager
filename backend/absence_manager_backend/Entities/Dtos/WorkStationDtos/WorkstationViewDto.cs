using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.WorkStationDtos
{
    public class WorkstationViewDto
    {
        public string Id { get; set; }
        public string OfficeId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public decimal? PositionX { get; set; }
        public decimal? PositionY { get; set; }
    }
}
