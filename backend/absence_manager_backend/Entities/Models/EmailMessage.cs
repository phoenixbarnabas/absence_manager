using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models
{
    public class EmailMessage
    {
        public required string To { get; init; }

        public string? Cc { get; init; }

        public string? Bcc { get; init; }

        public required string Subject { get; init; }

        public required string BodyHtml { get; init; }

        public string? BodyText { get; init; }
    }
}
