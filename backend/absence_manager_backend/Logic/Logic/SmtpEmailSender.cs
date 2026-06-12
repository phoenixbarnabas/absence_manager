using Entities.Helpers;
using Entities.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Logic.Logic
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(
            IOptions<EmailSettings> settings,
            ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (!_settings.Enabled)
            {
                _logger.LogInformation(
                    "Email sending is disabled. Email was not sent. To: {Recipient}, Subject: {Subject}",
                    message.To,
                    message.Subject);

                return;
            }

            ValidateSettings();

            using var mailMessage = CreateMailMessage(message);

            using var smtpClient = CreateSmtpClient();

            try
            {
                await smtpClient.SendMailAsync(mailMessage, cancellationToken);

                _logger.LogInformation(
                    "Email sent successfully. To: {Recipient}, Subject: {Subject}",
                    message.To,
                    message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Email sending failed. To: {Recipient}, Subject: {Subject}",
                    message.To,
                    message.Subject);

                throw;
            }
        }

        private MailMessage CreateMailMessage(EmailMessage message)
        {
            var fromAddress = new MailAddress(
                _settings.FromAddress!,
                _settings.FromName);

            var mailMessage = new MailMessage
            {
                From = fromAddress,
                Subject = message.Subject,
                Body = message.BodyHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(message.To);

            if (!string.IsNullOrWhiteSpace(message.Cc))
            {
                mailMessage.CC.Add(message.Cc);
            }

            if (!string.IsNullOrWhiteSpace(message.Bcc))
            {
                mailMessage.Bcc.Add(message.Bcc);
            }

            return mailMessage;
        }

        private SmtpClient CreateSmtpClient()
        {
            var smtpSettings = _settings.Smtp;

            var smtpClient = new SmtpClient(smtpSettings.Host!, smtpSettings.Port)
            {
                EnableSsl = smtpSettings.EnableSsl,
                Timeout = smtpSettings.TimeoutMilliseconds,
                UseDefaultCredentials = smtpSettings.UseDefaultCredentials
            };

            if (!smtpSettings.UseDefaultCredentials &&
                !string.IsNullOrWhiteSpace(smtpSettings.Username))
            {
                smtpClient.Credentials = new NetworkCredential(
                    smtpSettings.Username,
                    smtpSettings.Password);
            }

            return smtpClient;
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.FromAddress))
            {
                throw new InvalidOperationException("Email FromAddress is required when email sending is enabled.");
            }

            if (string.IsNullOrWhiteSpace(_settings.Smtp.Host))
            {
                throw new InvalidOperationException("SMTP Host is required when email sending is enabled.");
            }

            if (_settings.Smtp.Port <= 0)
            {
                throw new InvalidOperationException("SMTP Port must be greater than zero.");
            }
        }
    }
}
