using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.UserActivityLogDtos
{
    public class UserActivityLogListResultDto
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public IReadOnlyList<UserActivityLogViewDto> Items { get; set; } = Array.Empty<UserActivityLogViewDto>();
    }
}
