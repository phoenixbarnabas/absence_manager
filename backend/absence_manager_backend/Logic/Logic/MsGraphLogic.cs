using Entities.Dtos.Graph;
using Microsoft.Graph;
using GraphUser = Microsoft.Graph.Models.User;
using Microsoft.Kiota.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Logic
{
    public interface IMsGraphLogic
    {
        Task<GraphUserProfileDto?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);

        Task<GraphUserDto?> GetUserByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default);

        Task<GraphUserDto?> GetManagerAsync(string entraObjectId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GraphUserDto>> GetDirectReportsAsync( string entraObjectId, CancellationToken cancellationToken = default);

        Task<GraphUserHierarchyDto> GetUserHierarchyAsync(string entraObjectId, CancellationToken cancellationToken = default);
    }

    public class MsGraphLogic : IMsGraphLogic
    {
        private static readonly string[] UserSelectFields =
        {
            "id",
            "displayName",
            "mail",
            "userPrincipalName",
            "department",
            "jobTitle",
            "officeLocation",
            "preferredLanguage"
        };

        private readonly GraphServiceClient _graphServiceClient;

        public MsGraphLogic(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<GraphUserProfileDto?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
        {
            var user = await _graphServiceClient.Me.GetAsync(config =>
            {
                config.QueryParameters.Select = UserSelectFields;
            }, cancellationToken);

            if (user == null)
                return null;

            return new GraphUserProfileDto
            {
                DisplayName = user.DisplayName,
                Email = user.Mail ?? user.UserPrincipalName,
                Department = user.Department,
                JobTitle = user.JobTitle,
                OfficeLocation = user.OfficeLocation,
                PreferredLanguage = user.PreferredLanguage
            };
        }

        public async Task<GraphUserDto?> GetUserByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default)
        {
            ValidateEntraObjectId(entraObjectId);

            try
            {
                var user = await _graphServiceClient.Users[entraObjectId].GetAsync(config =>
                {
                    config.QueryParameters.Select = UserSelectFields;
                }, cancellationToken);

                return ToGraphUserDto(user);
            }
            catch (ApiException ex) when (ex.ResponseStatusCode == 404)
            {
                return null;
            }
            catch (ApiException ex)
            {
                throw new InvalidOperationException(
                    $"Graph user retrieval failed. Status: {ex.ResponseStatusCode}",
                    ex);
            }
        }

        public async Task<GraphUserDto?> GetManagerAsync(string entraObjectId, CancellationToken cancellationToken = default)
        {
            ValidateEntraObjectId(entraObjectId);

            try
            {
                var manager = await _graphServiceClient.Users[entraObjectId].Manager.GetAsync(config =>
                {
                    config.QueryParameters.Select = UserSelectFields;
                }, cancellationToken);

                return ToGraphUserDto(manager as GraphUser);
            }
            catch (ApiException ex) when (ex.ResponseStatusCode == 404)
            {
                return null;
            }
            catch (ApiException ex)
            {
                throw new InvalidOperationException(
                    $"Graph manager retrieval failed. Status: {ex.ResponseStatusCode}",
                    ex);
            }
        }

        public async Task<IReadOnlyList<GraphUserDto>> GetDirectReportsAsync(string entraObjectId, CancellationToken cancellationToken = default)
        {
            ValidateEntraObjectId(entraObjectId);

            try
            {
                var response = await _graphServiceClient.Users[entraObjectId].DirectReports.GetAsync(config =>
                {
                    config.QueryParameters.Select = UserSelectFields;
                }, cancellationToken);

                if (response?.Value == null || response.Value.Count == 0)
                    return Array.Empty<GraphUserDto>();

                return response.Value
                    .OfType<GraphUser>()
                    .Select(ToGraphUserDto)
                    .Where(x => x != null)
                    .Cast<GraphUserDto>()
                    .ToList();
            }
            catch (ApiException ex)
            {
                throw new InvalidOperationException(
                    $"Graph direct reports retrieval failed. Status: {ex.ResponseStatusCode}",
                    ex);
            }
        }

        public async Task<GraphUserHierarchyDto> GetUserHierarchyAsync(string entraObjectId, CancellationToken cancellationToken = default)
        {
            ValidateEntraObjectId(entraObjectId);

            var currentUser = await GetUserByEntraObjectIdAsync(entraObjectId, cancellationToken);
            var manager = await GetManagerAsync(entraObjectId, cancellationToken);
            var directReports = await GetDirectReportsAsync(entraObjectId, cancellationToken);

            return new GraphUserHierarchyDto
            {
                CurrentUser = currentUser,
                Manager = manager,
                DirectReports = directReports
            };
        }

        private static GraphUserDto? ToGraphUserDto(GraphUser? user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
                return null;

            return new GraphUserDto
            {
                EntraObjectId = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Mail ?? user.UserPrincipalName,
                UserPrincipalName = user.UserPrincipalName,
                Department = user.Department,
                JobTitle = user.JobTitle,
                OfficeLocation = user.OfficeLocation
            };
        }

        private static void ValidateEntraObjectId(string entraObjectId)
        {
            if (string.IsNullOrWhiteSpace(entraObjectId))
                throw new ArgumentException("Entra object id is required.", nameof(entraObjectId));
        }
    }
}
