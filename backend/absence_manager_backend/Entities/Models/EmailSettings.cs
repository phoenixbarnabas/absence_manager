using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models
{
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public bool Enabled { get; set; } = false;

        public string Provider { get; set; } = "Smtp";

        public string? FromAddress { get; set; }

        public string FromName { get; set; } = "Távollétkezelő";

        public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

        public int ApprovalTokenExpirationHours { get; set; } = 72;

        public int MaxRetryCount { get; set; } = 3;

        public SmtpEmailSettings Smtp { get; set; } = new();
    }

    public class SmtpEmailSettings
    {
        public string? Host { get; set; }

        public int Port { get; set; } = 25;

        public bool EnableSsl { get; set; } = false;

        public bool UseDefaultCredentials { get; set; } = false;

        public string? Username { get; set; }

        public string? Password { get; set; }

        public int TimeoutMilliseconds { get; set; } = 300000;
    }
}
