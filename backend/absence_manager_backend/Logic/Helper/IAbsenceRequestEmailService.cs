using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Helper
{
    public interface IAbsenceRequestEmailService
    {
        Task SendManagerApprovalRequestEmailAsync(
            string absenceRequestId,
            CancellationToken cancellationToken = default);
    }
}
