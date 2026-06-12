using Entities.Dtos.AbsenceRequestDtos;
using Entities.Enums;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Helper
{
    public interface IAbsenceRequestActionTokenService
    {
        Task<AbsenceRequestActionTokensDto> CreateTokensAsync(
            string absenceRequestId,
            string managerUserId,
            CancellationToken cancellationToken = default);

        Task<AbsenceRequestActionToken> GetValidTokenAsync(
            string rawToken,
            AbsenceRequestEmailAction? expectedAction = null,
            CancellationToken cancellationToken = default);

        Task MarkTokensUsedForRequestAsync(
            string absenceRequestId,
            string managerUserId,
            CancellationToken cancellationToken = default);
    }
}
