using Data;
using Entities.Enums;
using Entities.Helpers;
using Entities.Models;
using Logic.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Logic.Logic
{
    public class AbsenceRequestEmailService : IAbsenceRequestEmailService
    {
        private readonly AbsenceManagerDbContext _dbContext;
        private readonly IEmailSender _emailSender;
        private readonly IAbsenceRequestActionTokenService _tokenService;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<AbsenceRequestEmailService> _logger;
        private readonly IHostEnvironment _environment;

        public AbsenceRequestEmailService(
            AbsenceManagerDbContext dbContext,
            IEmailSender emailSender,
            IAbsenceRequestActionTokenService tokenService,
            IOptions<EmailSettings> emailSettings,
            ILogger<AbsenceRequestEmailService> logger,
            IHostEnvironment environment)

        {
            _dbContext = dbContext;
            _emailSender = emailSender;
            _tokenService = tokenService;
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _environment = environment;
        }

        public async Task SendManagerApprovalRequestEmailAsync(
            string absenceRequestId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_emailSettings.Enabled)
                {
                    await CreateSkippedEmailLogAsync(
                        absenceRequestId,
                        recipientEmail: "(email-disabled)",
                        recipientName: null,
                        subject: "Új szabadságkérelem érkezett jóváhagyásra",
                        reason: "Email sending is disabled in configuration.",
                        cancellationToken);

                    return;
                }

                var absenceRequest = await _dbContext.AbsenceRequests
                    .AsNoTracking()
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == absenceRequestId, cancellationToken);

                if (absenceRequest == null)
                {
                    _logger.LogWarning(
                        "Manager approval email was not sent because absence request was not found. AbsenceRequestId={AbsenceRequestId}",
                        absenceRequestId);

                    return;
                }

                if (absenceRequest.Status != AbsenceRequestStatus.Pending)
                {
                    await CreateSkippedEmailLogAsync(
                        absenceRequest.Id,
                        recipientEmail: "(not-pending)",
                        recipientName: null,
                        subject: "Új szabadságkérelem érkezett jóváhagyásra",
                        reason: $"Email was skipped because absence request status is {absenceRequest.Status}.",
                        cancellationToken);

                    return;
                }

                var manager = await ResolveManagerForEmailAsync(absenceRequest, cancellationToken);

                if (manager == null)
                {
                    await CreateSkippedEmailLogAsync(
                        absenceRequest.Id,
                        recipientEmail: "(missing-manager)",
                        recipientName: null,
                        subject: "Új szabadságkérelem érkezett jóváhagyásra",
                        reason: "Email was skipped because the requester has no active manager relation.",
                        cancellationToken);

                    return;
                }

                if (string.IsNullOrWhiteSpace(manager.Email))
                {
                    await CreateSkippedEmailLogAsync(
                        absenceRequest.Id,
                        recipientEmail: "(missing-manager-email)",
                        recipientName: manager.DisplayName,
                        subject: "Új szabadságkérelem érkezett jóváhagyásra",
                        reason: "Email was skipped because the active manager has no email address.",
                        cancellationToken);

                    return;
                }

                var actionTokens = await _tokenService.CreateTokensAsync(
                    absenceRequest.Id,
                    manager.Id,
                    cancellationToken);

                var approveUrl = BuildEmailActionUrl(actionTokens.ApproveToken);
                var rejectUrl = BuildEmailActionUrl(actionTokens.RejectToken);

                var subject = $"Új szabadságkérelem érkezett: {absenceRequest.User.DisplayName}";

                var bodyHtml = BuildManagerApprovalEmailBody(
                    absenceRequest,
                    manager,
                    approveUrl,
                    rejectUrl,
                    actionTokens.ExpiresAtUtc);

                var emailLog = new EmailLog
                {
                    NotificationType = EmailNotificationTypes.AbsenceRequestManagerApproval,
                    Status = EmailLogStatus.Pending,
                    RecipientEmail = manager.Email,
                    RecipientName = manager.DisplayName,
                    SenderEmail = _emailSettings.FromAddress,
                    Subject = subject,
                    BodyHtml = bodyHtml,
                    Provider = _emailSettings.Provider,
                    AbsenceRequestId = absenceRequest.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                    AttemptCount = 0
                };

                _dbContext.EmailLogs.Add(emailLog);
                await _dbContext.SaveChangesAsync(cancellationToken);

                try
                {
                    emailLog.AttemptCount++;
                    emailLog.LastAttemptAtUtc = DateTime.UtcNow;

                    await _emailSender.SendAsync(
                        new EmailMessage
                        {
                            To = manager.Email,
                            Subject = subject,
                            BodyHtml = bodyHtml
                        },
                        cancellationToken);

                    emailLog.Status = EmailLogStatus.Sent;
                    emailLog.SentAtUtc = DateTime.UtcNow;
                    emailLog.ErrorMessage = null;
                }
                catch (Exception ex)
                {
                    emailLog.Status = EmailLogStatus.Failed;
                    emailLog.ErrorMessage = ex.Message;

                    _logger.LogError(
                        ex,
                        "Manager approval email sending failed. AbsenceRequestId={AbsenceRequestId}, ManagerUserId={ManagerUserId}",
                        absenceRequest.Id,
                        manager.Id);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Fontos: email hiba nem boríthatja el a szabadságkérelem létrehozását.
                _logger.LogError(
                    ex,
                    "Unexpected error while preparing manager approval email. AbsenceRequestId={AbsenceRequestId}",
                    absenceRequestId);
            }
        }

        private async Task<AppUser?> ResolveManagerForEmailAsync(AbsenceRequest absenceRequest,CancellationToken cancellationToken)
        {
            if (_environment.IsDevelopment() &&
                !string.IsNullOrWhiteSpace(_emailSettings.TestManagerOverrideEmail))
            {
                var overrideManager = await _dbContext.AppUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Email != null &&
                        x.Email.ToLower() == _emailSettings.TestManagerOverrideEmail.ToLower(),
                        cancellationToken);

                if (overrideManager == null)
                {
                    _logger.LogWarning(
                        "Test manager override email is configured, but no AppUser was found with this email. Email={Email}",
                        _emailSettings.TestManagerOverrideEmail);

                    return null;
                }

                _logger.LogWarning(
                    "Development manager email override is active. AbsenceRequestId={AbsenceRequestId}, OverrideManagerUserId={ManagerUserId}, OverrideEmail={Email}",
                    absenceRequest.Id,
                    overrideManager.Id,
                    overrideManager.Email);

                return overrideManager;
            }

            var managerRelation = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .Include(x => x.ManagerUser)
                .Where(x =>
                    x.UserId == absenceRequest.UserId &&
                    x.IsActive)
                .OrderByDescending(x => x.ValidFromUtc)
                .FirstOrDefaultAsync(cancellationToken);

            return managerRelation?.ManagerUser;
        }

        private async Task CreateSkippedEmailLogAsync(
            string? absenceRequestId,
            string recipientEmail,
            string? recipientName,
            string subject,
            string reason,
            CancellationToken cancellationToken)
        {
            var emailLog = new EmailLog
            {
                NotificationType = EmailNotificationTypes.AbsenceRequestManagerApproval,
                Status = EmailLogStatus.Skipped,
                RecipientEmail = recipientEmail,
                RecipientName = recipientName,
                SenderEmail = _emailSettings.FromAddress,
                Subject = subject,
                BodyHtml = reason,
                ErrorMessage = reason,
                Provider = _emailSettings.Provider,
                AbsenceRequestId = absenceRequestId,
                CreatedAtUtc = DateTime.UtcNow,
                AttemptCount = 0
            };

            _dbContext.EmailLogs.Add(emailLog);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private string BuildEmailActionUrl(string token)
        {
            var baseUrl = (_emailSettings.FrontendBaseUrl ?? string.Empty).TrimEnd('/');

            return $"{baseUrl}/absence-requests/email-action?token={WebUtility.UrlEncode(token)}";
        }

        private static string BuildManagerApprovalEmailBody(
            AbsenceRequest absenceRequest,
            AppUser manager,
            string approveUrl,
            string rejectUrl,
            DateTime expiresAtUtc)
        {
            var employeeName = Html(absenceRequest.User.DisplayName);
            var employeeEmail = Html(absenceRequest.User.Email ?? "-");
            var managerName = Html(manager.DisplayName);
            var typeLabel = Html(GetTypeLabel(absenceRequest.Type));
            var dateFrom = Html(absenceRequest.DateFrom.ToString("yyyy.MM.dd."));
            var dateTo = Html(absenceRequest.DateTo.ToString("yyyy.MM.dd."));
            var reason = Html(string.IsNullOrWhiteSpace(absenceRequest.Reason)
                ? "-"
                : absenceRequest.Reason);

            var expiresAt = Html(expiresAtUtc.ToLocalTime().ToString("yyyy.MM.dd. HH:mm"));

            var builder = new StringBuilder();

            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html lang=\"hu\">");
            builder.AppendLine("<body style=\"font-family: Arial, sans-serif; color: #1f2937; line-height: 1.5;\">");

            builder.AppendLine($"<p>Kedves {managerName}!</p>");

            builder.AppendLine($"<p>{employeeName} új szabadságkérelmet adott le, amely jóváhagyásra vár.</p>");

            builder.AppendLine("<table style=\"border-collapse: collapse; margin: 16px 0;\">");
            AppendRow(builder, "Kérelmező", employeeName);
            AppendRow(builder, "Email", employeeEmail);
            AppendRow(builder, "Távollét típusa", typeLabel);
            AppendRow(builder, "Kezdő dátum", dateFrom);
            AppendRow(builder, "Záró dátum", dateTo);
            AppendRow(builder, "Megjegyzés", reason);
            builder.AppendLine("</table>");

            builder.AppendLine("<p>A kérelmet az alábbi gombokkal tudod elbírálni:</p>");

            builder.AppendLine("<p style=\"margin: 24px 0;\">");
            builder.AppendLine($"<a href=\"{HtmlAttribute(approveUrl)}\" style=\"display: inline-block; padding: 10px 16px; margin-right: 8px; background: #166534; color: #ffffff; text-decoration: none; border-radius: 6px;\">Elfogadás</a>");
            builder.AppendLine($"<a href=\"{HtmlAttribute(rejectUrl)}\" style=\"display: inline-block; padding: 10px 16px; background: #991b1b; color: #ffffff; text-decoration: none; border-radius: 6px;\">Elutasítás</a>");
            builder.AppendLine("</p>");

            builder.AppendLine("<p style=\"font-size: 13px; color: #6b7280;\">");
            builder.AppendLine($"A linkek lejárati ideje: {expiresAt}.<br>");
            builder.AppendLine("</p>");

            builder.AppendLine("<p>Üdvözlettel,<br>Távollétkezelő</p>");

            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }

        private static void AppendRow(StringBuilder builder, string label, string value)
        {
            builder.AppendLine("<tr>");
            builder.AppendLine($"<td style=\"padding: 6px 12px 6px 0; font-weight: bold; vertical-align: top;\">{Html(label)}</td>");
            builder.AppendLine($"<td style=\"padding: 6px 0; vertical-align: top;\">{value}</td>");
            builder.AppendLine("</tr>");
        }

        private static string GetTypeLabel(AbsenceRequestType type)
        {
            return type switch
            {
                AbsenceRequestType.Vacation => "Szabadság",
                AbsenceRequestType.HomeOffice => "Home office",
                AbsenceRequestType.SickLeave => "Betegszabadság",
                AbsenceRequestType.OtherAbsence => "Egyéb távollét",
                _ => "Távollét"
            };
        }

        private static string Html(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string HtmlAttribute(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
