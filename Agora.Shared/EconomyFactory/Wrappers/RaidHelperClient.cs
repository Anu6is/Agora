﻿using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Emporia.Extensions.Discord;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agora.Shared.EconomyFactory
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Singleton)]
    public class RaidHelperClient : AgoraService
    {
        public const string SectionName = "RaidHelper";

        private readonly IGuildSettingsService _settingsService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RaidHelperClient(IGuildSettingsService settingsService,
                                IHttpClientFactory clientFactory,
                                IConfiguration configuration,
                                ILogger<RaidHelperClient> logger) : base(logger)
        {
            _settingsService = settingsService;
            _httpClientFactory = clientFactory;
            _configuration = configuration;
        }

        public async Task<RaidHelperResponse> GetUserBalanceAsync(ulong guildId, ulong userId)
        {
            return await SendRequestAsync(HttpMethod.Get, guildId.ToString(), userId.ToString());
        }

        public async Task<RaidHelperResponse> SetUserBalanceAsync(ulong guildId, ulong userId, decimal value, string reason = null)
        {
            return await SendRequestAsync(HttpMethod.Patch, guildId.ToString(), userId.ToString(), new 
            { 
                operation = "set", 
                value = value.ToString(),
                description = reason ?? string.Empty
            });
        }

        public async Task<RaidHelperResponse> IncreaseUserBalanceAsync(ulong guildId, ulong userId, decimal value, string reason = null)
        {
            return await SendRequestAsync(HttpMethod.Patch, guildId.ToString(), userId.ToString(), new
            {
                operation = "add",
                value = value.ToString(),
                description = reason ?? string.Empty
            });
        }

        public async Task<RaidHelperResponse> DecreaseUserBalanceAsync(ulong guildId, ulong userId, decimal value, string reason = null)
        {
            return await SendRequestAsync(HttpMethod.Patch, guildId.ToString(), userId.ToString(), new
            {
                operation = "subtract",
                value = value.ToString(),
                description = reason ?? string.Empty
            });
        }

        private async Task<RaidHelperResponse> SendRequestAsync(HttpMethod method, string serverId, string userId, object requestBody = null)
        {
            var settings = await _settingsService.GetGuildSettingsAsync(ulong.Parse(serverId));

            if (!settings.ExternalApiKeys.TryGetValue(serverId, out var apiKey))
                throw new UnauthorizedAccessException("Auction Bot needs to be authorized to use Raid-Helper DKP in this server!");

            var url = _configuration[$"Url:{RaidHelperClient.SectionName}"];

            using var httpClient = _httpClientFactory.CreateClient(RaidHelperClient.SectionName);
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

            using var request = new HttpRequestMessage(method, $"servers/{serverId}/entities/{userId}/dkp");

            if (requestBody != null)
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new ValidationException($"Failed to process DKP request: {response.ReasonPhrase}");

            var responseContent = await response.Content.ReadAsStringAsync();
            
            return JsonSerializer.Deserialize<ResponseList>(responseContent).Results.First();
        }
    }

    public class RaidHelperResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("dkp")]
        public string Dkp { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class ResponseList
    {
        [JsonPropertyName("result")]
        public List<RaidHelperResponse> Results { get; set; }
    }
}