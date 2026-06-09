using Data;
using Entities.Dtos.AbsenceRequestDtos;
using Entities.Enums;
using Entities.Models;
using Logic.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Logic.Logic
{
    public class AbsenceRequestActionTokenService : IAbsenceRequestActionTokenService
    {
        private const int TokenByteLength = 32;

        private readonly AbsenceManagerDbContext _dbContext;
        private readonly EmailSettings _emailSettings;

        public AbsenceRequestActionTokenService(
            AbsenceManagerDbContext dbContext,
            IOptions<EmailSettings> emailSettings)
        {
            _dbContext = dbContext;
            _emailSettings = emailSettings.Value;
        }

        public async Task<AbsenceRequestActionTokensDto> CreateTokensAsync(
            string absenceRequestId,
            string managerUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(absenceRequestId))
            {
                throw new ArgumentException("Absence request id is required.", nameof(absenceRequestId));
            }

            if (string.IsNullOrWhiteSpace(managerUserId))
            {
                throw new ArgumentException("Manager user id is required.", nameof(managerUserId));
            }

            var absenceRequest = await _dbContext.AbsenceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == absenceRequestId, cancellationToken)
                ?? throw new KeyNotFoundException("Absence request was not found.");

            if (absenceRequest.Status != AbsenceRequestStatus.Pending)
            {
                throw new InvalidOperationException("Only pending absence requests can have email action tokens.");
            }

            var isActiveManager = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == absenceRequest.UserId &&
                    x.ManagerUserId == managerUserId &&
                    x.IsActive,
                    cancellationToken);

            if (!isActiveManager)
            {
                throw new UnauthorizedAccessException("The selected user is not an active manager for this absence request.");
            }

            var now = DateTime.UtcNow;
            var expiresAtUtc = now.AddHours(_emailSettings.ApprovalTokenExpirationHours);

            var previousTokens = await _dbContext.AbsenceRequestActionTokens
                .Where(x =>
                    x.AbsenceRequestId == absenceRequestId &&
                    x.ManagerUserId == managerUserId &&
                    !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var previousToken in previousTokens)
            {
                previousToken.IsUsed = true;
                previousToken.UsedAtUtc = now;
            }

            var approveRawToken = GenerateRawToken();
            var rejectRawToken = GenerateRawToken();

            var approveToken = new AbsenceRequestActionToken
            {
                AbsenceRequestId = absenceRequestId,
                ManagerUserId = managerUserId,
                Action = AbsenceRequestEmailAction.Approve,
                TokenHash = HashToken(approveRawToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = expiresAtUtc,
                IsUsed = false
            };

            var rejectToken = new AbsenceRequestActionToken
            {
                AbsenceRequestId = absenceRequestId,
                ManagerUserId = managerUserId,
                Action = AbsenceRequestEmailAction.Reject,
                TokenHash = HashToken(rejectRawToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = expiresAtUtc,
                IsUsed = false
            };

            _dbContext.AbsenceRequestActionTokens.AddRange(approveToken, rejectToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AbsenceRequestActionTokensDto
            {
                ApproveToken = approveRawToken,
                RejectToken = rejectRawToken,
                ExpiresAtUtc = expiresAtUtc
            };
        }

        public async Task<AbsenceRequestActionToken> GetValidTokenAsync(
            string rawToken,
            AbsenceRequestEmailAction? expectedAction = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                throw new ArgumentException("Token is required.", nameof(rawToken));
            }

            var tokenHash = HashToken(rawToken);
            var now = DateTime.UtcNow;

            var token = await _dbContext.AbsenceRequestActionTokens
                .Include(x => x.AbsenceRequest)
                .ThenInclude(x => x.User)
                .Include(x => x.ManagerUser)
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
                ?? throw new KeyNotFoundException("The email action link is invalid.");

            if (expectedAction.HasValue && token.Action != expectedAction.Value)
            {
                throw new InvalidOperationException("The email action link is not valid for this action.");
            }

            if (token.IsUsed)
            {
                throw new InvalidOperationException("The email action link has already been used.");
            }

            if (token.ExpiresAtUtc <= now)
            {
                throw new InvalidOperationException("The email action link has expired.");
            }

            if (token.AbsenceRequest.Status != AbsenceRequestStatus.Pending)
            {
                throw new InvalidOperationException("Only pending absence requests can be reviewed from email.");
            }

            var isActiveManager = await _dbContext.AppUserManagerRelations
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == token.AbsenceRequest.UserId &&
                    x.ManagerUserId == token.ManagerUserId &&
                    x.IsActive,
                    cancellationToken);

            if (!isActiveManager)
            {
                throw new UnauthorizedAccessException("The manager relation is no longer active for this absence request.");
            }

            return token;
        }

        public async Task MarkTokensUsedForRequestAsync(
            string absenceRequestId,
            string managerUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(absenceRequestId))
            {
                throw new ArgumentException("Absence request id is required.", nameof(absenceRequestId));
            }

            if (string.IsNullOrWhiteSpace(managerUserId))
            {
                throw new ArgumentException("Manager user id is required.", nameof(managerUserId));
            }

            var now = DateTime.UtcNow;

            var tokens = await _dbContext.AbsenceRequestActionTokens
                .Where(x =>
                    x.AbsenceRequestId == absenceRequestId &&
                    x.ManagerUserId == managerUserId &&
                    !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.IsUsed = true;
                token.UsedAtUtc = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static string GenerateRawToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(TokenByteLength);

            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string HashToken(string rawToken)
        {
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hashBytes = SHA256.HashData(bytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
