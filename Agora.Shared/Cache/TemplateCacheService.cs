using Agora.Shared.Services;
using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ZiggyCreatures.Caching.Fusion;

namespace Agora.Shared.Cache
{
    public class TemplateCacheService : AgoraService, ITemplateCacheService
    {
        private const int CacheExpirationInSeconds = 5;

        private readonly IFusionCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> Tokens;

        public TemplateCacheService(IFusionCache cache, ILogger<IGuildSettingsService> logger, IServiceScopeFactory factory) : base(logger)
        {
            Tokens = new();
            _cache = cache;
            _scopeFactory = factory;
        }

        public ValueTask<AuctionTemplate> GetAuctionTemplateAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IEnumerable<AuctionTemplate>> GetAuctionTemplatesAsync(ulong guildId)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());

            return await _cache.GetOrSetAsync(
                $"auction:{guildId}",
                async cts =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var result = await mediator.Send(new GetAuctionTemplateListQuery(guildId), cts);

                    if (result.Data is not null) return result.Data;

                    return Array.Empty<AuctionTemplate>();
                },
                TimeSpan.FromSeconds(CacheExpirationInSeconds),
                Tokens[guildId].Token);
        }
    }
}
