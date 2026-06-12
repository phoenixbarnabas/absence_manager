using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Dtos.AbsenceRequestDtos
{
    public class AbsenceRequestActionTokensDto
    {
        public required string ApproveToken { get; set; }

        public required string RejectToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
    }
}
