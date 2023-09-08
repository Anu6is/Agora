using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;

namespace Agora.API.Services
{
    public class AccessTokenService
    {
        private const string Provider = "oauth2|DiscordGuilds|";
        private readonly IConfiguration _configuration;

        private AsyncLazy<string> Token { get; set; }

        public AccessTokenService(IConfiguration configuration)
        {
            _configuration = configuration;

            Token = new(async () => await GenerateAccessTokenAsync(configuration));
        }

        public async Task<string> GetDiscordTokenAsync(string id)
        {
            var client = new ManagementApiClient(await Token, new Uri(_configuration["Auth0:API:audience"]!));

            var user = await client.Users.GetAsync($"{Provider}{id}");

            return user.Identities.First().AccessToken;
        }

        private static async Task<string> GenerateAccessTokenAsync(IConfiguration configuration)
        {
            var client = new AuthenticationApiClient(configuration["Auth0:Domain"]);
            var tokenResponse = await client.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                ClientId = configuration["Auth0:API:ClientId"],
                ClientSecret = configuration["Auth0:API:ClientSecret"],
                Audience = configuration["Auth0:API:Audience"]
            });

            return tokenResponse.AccessToken;
        }
    }
}