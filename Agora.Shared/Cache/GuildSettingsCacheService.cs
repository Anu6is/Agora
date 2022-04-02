using Agora.Shared.Services;
using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Agora.Shared.Cache
{
    public class GuildSettingsCacheService : AgoraService, IGuildSettingsService
    {
        private const int CacheExpirationInMinutes = 15;
        
        private readonly IFusionCache _settingsCache;
        private readonly IServiceProvider _serviceProvider;

        public GuildSettingsCacheService(IFusionCache cache, ILogger<IGuildSettingsService> logger, IServiceProvider services) : base(logger)
        {
            _settingsCache = cache;
            _serviceProvider = services;
        }

        public async ValueTask AddGuildSettingsAsync(IDiscordGuildSettings settings)
            => await _settingsCache.SetAsync($"settings:{settings.GuildId}", settings, TimeSpan.FromMinutes(CacheExpirationInMinutes));

        public async ValueTask<IDiscordGuildSettings> GetGuildSettingsAsync(ulong guildId)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await _settingsCache.GetOrSetAsync<IDiscordGuildSettings>(
                           $"settings:{guildId}",
                           async cts =>
                           {
                               var result = await mediator.Send(new GetGuildSettingsDetailsQuery(guildId), cts);
                               return result.Data;
                           },
                           TimeSpan.FromMinutes(CacheExpirationInMinutes));
        }
    }
}
