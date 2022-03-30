using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Commands;
using Emporia.Extensions.Discord.Features.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Agora.Discord.Services.Standard
{
    public class GuildSettingsService : AgoraService, IGuildSettingsService
    {
        private readonly IFusionCache _settingsCache;
        private readonly IServiceProvider _serviceProvider;

        public GuildSettingsService(IFusionCache cache, ILogger<IGuildSettingsService> logger, IServiceProvider services) : base(logger)
        {
            _settingsCache = cache;
            _serviceProvider = services;
        }

        public async ValueTask AddGuildSettingsAsync(IDiscordGuildSettings settings) 
            => await _settingsCache.SetAsync($"settings:{settings.GuildId}", settings, TimeSpan.FromMinutes(10));

        public async ValueTask<IDiscordGuildSettings> GetGuildSettingsAsync(ulong guildId)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await _settingsCache.GetOrSetAsync<IDiscordGuildSettings>(
                           $"settings:{guildId}",
                           async _ =>
                           {
                               var result = await mediator.Send(new GetGuildSettingsDetailsQuery(guildId));
                               return result.Data;
                           },
                           TimeSpan.FromMinutes(10)
                       );
        }
        
        public async ValueTask<IDiscordGuildSettings> UpdateGuildSettingsAsync(IDiscordGuildSettings settings)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new UpdateGuildSettingsCommand((DefaultDiscordGuildSettings)settings));
        }
    }
}
