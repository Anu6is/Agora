using Agora.Shared.Services;
using Emporia.Application.Features.Commands;
using Emporia.Application.Features.Queries;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ZiggyCreatures.Caching.Fusion;

namespace Agora.Shared.Cache
{
    public class EporiaCacheService : AgoraService, IEmporiaCacheService
    {
        private const int ShortCacheExpirtionInMinutes = 2;
        private const int LongCacheExpirationInMinutes = 15;

        private readonly IFusionCache _emporiumCache;
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> Tokens;

        public EporiaCacheService(IFusionCache cache, ILogger<IGuildSettingsService> logger, IServiceProvider services) : base(logger)
        {
            Tokens = new();
            _emporiumCache = cache;
            _serviceProvider = services;
        }

        public void Clear(ulong guildId)
        {
            _emporiumCache.Remove($"emporium:{guildId}");

            if (Tokens.TryRemove(guildId, out var source))
                source.Cancel();
        }

        public async ValueTask AddEmporiumAsync(Emporium emporium)
        {
            if (!Tokens.ContainsKey(emporium.Id.Value))
                Tokens.TryAdd(emporium.Id.Value, new CancellationTokenSource());

            await _emporiumCache.SetAsync($"emporium:{emporium.Id.Value}",
                                                     new CachedEmporium()
                                                     {
                                                         EmporiumId = emporium.Id.Value,
                                                         Categories = emporium.Categories.ToList(),
                                                         Currencies = emporium.Currencies.ToList(),
                                                         Showrooms = emporium.Showrooms.ToList(),
                                                         TimeOffset = emporium.TimeOffset
                                                     },
                                                     TimeSpan.FromMinutes(LongCacheExpirationInMinutes),
                                                     Tokens[emporium.Id.Value].Token);
            return;
        }

        public async ValueTask RemoveEmporiumAsync(ulong guildId)
            => await _emporiumCache.RemoveAsync($"emporium:{guildId}");

        public CachedEmporium GetCachedEmporium(ulong guildId)
            => _emporiumCache.GetOrDefault<CachedEmporium>($"emporium:{guildId}", token: Tokens[guildId].Token);

        public async ValueTask<CachedEmporium> GetEmporiumAsync(ulong guildId)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());

            return await _emporiumCache.GetOrSetAsync(
                           $"emporium:{guildId}",
                           async cts =>
                           {
                               using var scope = _serviceProvider.CreateScope();
                               var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                               var result = await mediator.Send(new GetEmporiumDetailsQuery(new EmporiumId(guildId)) { Cache = true }, cts);

                               if (result.Data == null) return null;

                               return new CachedEmporium()
                               {
                                   EmporiumId = result.Data.EmporiumId,
                                   Categories = result.Data.Categories.ToList(),
                                   Currencies = result.Data.Currencies.ToList(),
                                   Showrooms = result.Data.Showrooms.ToList(),
                                   TimeOffset = result.Data.TimeOffset
                               };
                           },
                           TimeSpan.FromMinutes(LongCacheExpirationInMinutes),
                           Tokens[guildId].Token);
        }

        public ValueTask RemoveUserAsync(ulong guildId, ulong userId)
            => _emporiumCache.RemoveAsync($"user:{guildId}:{userId}");

        public ValueTask<CachedEmporiumUser> GetUserAsync(ulong guildId, ulong userId)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());

            return _emporiumCache.GetOrSetAsync(
                $"user:{guildId}:{userId}",
                async cts =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var result = await mediator.Send(new GetEmporiumUserDetailsQuery(new EmporiumId(guildId), ReferenceNumber.Create(userId)) { Cache = false }, cts);

                    if (result.Data == null)
                    {
                        var newUser = await mediator.Send(new CreateEmporiumUserCommand(new EmporiumId(guildId), ReferenceNumber.Create(userId)), cts);
                        var userDetails = new CachedEmporiumUser()
                        {
                            UserId = newUser.Id.Value,
                            EmporiumId = newUser.EmporiumId.Value,
                            ReferenceNumber = newUser.ReferenceNumber.Value
                        };

                        return userDetails;
                    }

                    return new CachedEmporiumUser()
                    {
                        UserId = result.Data.UserId,
                        EmporiumId = result.Data.EmporiumId,
                        ReferenceNumber = result.Data.ReferenceNumber
                    };
                },
                TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes),
                Tokens[guildId].Token);
        }
    }
}
