using Entities.Dtos.Graph;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Logic
{
    public interface IMsGraphLogic
    {
        Task<GraphUserProfileDto?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);
    }

    public class MsGraphLogic : IMsGraphLogic
    {
        private readonly GraphServiceClient _graphServiceClient;

        public MsGraphLogic(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<GraphUserProfileDto?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
        {
            var user = await _graphServiceClient.Me.GetAsync(config =>
            {
                config.QueryParameters.Select = new[]
                {
                    "displayName",
                    "mail",
                    "userPrincipalName",
                    "department",
                    "jobTitle",
                    "officeLocation",
                    "preferredLanguage"
                };
            }, cancellationToken);

            Console.WriteLine($"Graph displayName: {user?.DisplayName}");
            Console.WriteLine($"Graph mail: {user?.Mail}");
            Console.WriteLine($"Graph department: {user?.Department}");
            Console.WriteLine($"Graph jobTitle: {user?.JobTitle}");

            if (user == null)
            {
                return null;
            }

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
    }
}
