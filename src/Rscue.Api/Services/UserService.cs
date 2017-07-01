namespace Rscue.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rscue.Api.Models;
    using Microsoft.Extensions.Options;
    using Rscue.Api.Plumbing;
    using Auth0.ManagementApi;
    using Auth0.ManagementApi.Models;
    using System.Dynamic;

    public class UserService : IUserService
    {
        private readonly IOptions<Auth0Settings> _auth0Settings;

        public UserService(IOptions<Auth0Settings> auth0Settings)
        {
            _auth0Settings = auth0Settings ?? throw new ArgumentNullException(nameof(auth0Settings));
        }

        public async Task<UserRegistration> RegisterUserAsync(UserRegistration userRegistration)
        {
            var managementUserToken = _auth0Settings.Value.ManagementUserToken;
            var domain = _auth0Settings.Value.Domain;
            var client = new ManagementApiClient(_auth0Settings.Value.ManagementUserToken, new Uri($"https://{domain}/api/v2"));
            var userRequest = new UserCreateRequest
            {
                FullName = $"{userRegistration.FirstName} {userRegistration.LastName}",
                Email = userRegistration.Email,
                Password = userRegistration.Password,
                Connection = Auth0Settings.Connection,
                FirstName = userRegistration.FirstName,
                LastName = userRegistration.LastName,
                EmailVerified = true,
                UserMetadata = new ExpandoObject(),
            };

            if (userRegistration.ProviderId != null)
            {
                userRequest.UserMetadata.api_provider_id = userRegistration.ProviderId;
            }

            if (userRegistration.ClientId != null)
            {
                userRequest.UserMetadata.api_client_id = userRegistration.ClientId;
            }

            if (userRegistration.WorkerId != null)
            {
                userRequest.UserMetadata.api_worker_id = userRegistration.WorkerId;
            }

            var result = await client.Users.CreateAsync(userRequest);
            userRegistration.Id = result.UserId;
            return userRegistration;
        }
    }
}
