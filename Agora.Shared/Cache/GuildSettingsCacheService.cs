using Agora.Shared.Services;
using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Commands;
using Emporia.Extensions.Discord.Features.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ZiggyCreatures.Caching.Fusion;

namespace Agora.Shared.Cache
{
    public class GuildSettingsCacheService : AgoraService, IGuildSettingsService
    {
        private const int CacheExpirationInMinutes = 15;

        private readonly IFusionCache _settingsCache;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> Tokens;

        public GuildSettingsCacheService(IFusionCache cache, ILogger<IGuildSettingsService> logger, IServiceScopeFactory factory) : base(logger)
        {
            Tokens = new();
            _settingsCache = cache;
            _scopeFactory = factory;
        }

        public void Clear(ulong guildId)
        {
            _settingsCache.Remove($"settings:{guildId}");

            if (Tokens.TryRemove(guildId, out var source))
                source.Cancel();
        }

        public async ValueTask AddGuildSettingsAsync(IDiscordGuildSettings settings)
        {
            if (!Tokens.ContainsKey(settings.GuildId))
                Tokens.TryAdd(settings.GuildId, new CancellationTokenSource());

            await _settingsCache.SetAsync($"settings:{settings.GuildId}",
                                          settings,
                                          TimeSpan.FromMinutes(CacheExpirationInMinutes),
                                          Tokens[settings.GuildId].Token);
        }

        public async ValueTask<IDiscordGuildSettings> GetGuildSettingsAsync(ulong guildId)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());

            return await _settingsCache.GetOrSetAsync<IDiscordGuildSettings>(
                           $"settings:{guildId}",
                           async cts =>
                           {
                               using var scope = _scopeFactory.CreateScope();
                               var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                               var result = await mediator.Send(new GetGuildSettingsDetailsQuery(guildId), cts);

                               return result.Data;
                           },
                           TimeSpan.FromMinutes(CacheExpirationInMinutes),
                           Tokens[guildId].Token);
        }

        public async ValueTask UpdateGuildSettingsAync(IDiscordGuildSettings settings)
        {
            using var scope = _scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new UpdateGuildSettingsCommand((DefaultDiscordGuildSettings)settings));
        }
    }
}
