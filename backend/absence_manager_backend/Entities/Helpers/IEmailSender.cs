using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Helpers
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }
}
