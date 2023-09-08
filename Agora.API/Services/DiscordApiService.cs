using Agora.API.Features.Guilds;
using System.Net.Http.Json;

namespace Agora.API.Services
{
    public class DiscordApiService : IDisposable
    {
        public static Uri BaseURI { get; } = new("https://discord.com/api/v10", UriKind.Absolute);

        private readonly HttpClient _httpClient;
        private readonly AccessTokenService _tokenService;

        public DiscordApiService(HttpClient httpClient, AccessTokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<Guild>> GetGuildsAsync(string userId)
        {
            try
            {
                var token = await _tokenService.GetDiscordTokenAsync(userId);

                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);

                return await _httpClient.GetFromJsonAsync<IList<Guild>>("users/@me/guilds") ?? Array.Empty<Guild>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request Error - {e.GetType()}: {e.Message}");
                return Array.Empty<Guild>();
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
