using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.UserActivityLogDtos
{
    public class UserActivityLogFilterOptionsDto
    {
        public IReadOnlyList<UserActivityLogOptionDto> Actions { get; set; } =
            Array.Empty<UserActivityLogOptionDto>();

        public IReadOnlyList<UserActivityLogOptionDto> EntityTypes { get; set; } =
            Array.Empty<UserActivityLogOptionDto>();

        public IReadOnlyList<UserActivityLogOptionDto> Outcomes { get; set; } =
            Array.Empty<UserActivityLogOptionDto>();
    }

    public class UserActivityLogOptionDto
    {
        public string Value { get; set; } = null!;

        public string Label { get; set; } = null!;
    }
}
