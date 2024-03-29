﻿using Agora.Shared.Services;
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

        public async ValueTask<CachedEmporiumUser> GetUserAsync(ulong guildId, ulong userId)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());

            return await _emporiumCache.GetOrSetAsync(
                $"user:{guildId}:{userId}",
                async cts =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var result = await mediator.Send(new GetEmporiumUserDetailsQuery(new EmporiumId(guildId), ReferenceNumber.Create(userId)) { Cache = false }, cts);

                    if (result.Data == null)
                    {
                        var newUser = await mediator.Send(new CreateEmporiumUserCommand(new EmporiumId(guildId), ReferenceNumber.Create(userId)), cts);

                        if (!newUser.IsSuccessful) return null;

                        var user = newUser.Data;
                        var userDetails = new CachedEmporiumUser()
                        {
                            UserId = user.Id.Value,
                            EmporiumId = user.EmporiumId.Value,
                            ReferenceNumber = user.ReferenceNumber.Value
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

        public ValueTask RemoveProductAsync(ulong guildId, ulong productReference)
            => _emporiumCache.RemoveAsync($"product:{guildId}:{productReference}");

        public CachedEmporiumProduct GetCachedProduct(ulong guildId, ulong productId)
            => _emporiumCache.GetOrDefault<CachedEmporiumProduct>($"product:{guildId}:{productId}", token: Tokens[guildId].Token);

        public async ValueTask<CachedEmporiumProduct> GetProductAsync(ulong guildId, ulong showroomId,
                                                                       ulong productReference, bool uniqueRoom = false)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());

            return await _emporiumCache.GetOrSetAsync(
                $"product:{guildId}:{(uniqueRoom ? showroomId : productReference)}",
                async cts => await _serviceProvider.GetRequiredService<IProductService>().GetProductAsync(showroomId, productReference),
                TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes),
                Tokens[guildId].Token);
        }

        public async ValueTask AddShowroomListingAsync(Showroom showroom)
        {
            if (!Tokens.ContainsKey(showroom.EmporiumId.Value))
                Tokens.TryAdd(showroom.EmporiumId.Value, new CancellationTokenSource());

            await _emporiumCache.SetAsync($"listing:{showroom.Listings.First().Id.Value}",
                                                     showroom,
                                                     TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes),
                                                     Tokens[showroom.EmporiumId.Value].Token);
            return;
        }

        public async ValueTask<Showroom> GetShowroomListingAsync(Showroom showroom)
        {
            if (!Tokens.ContainsKey(showroom.EmporiumId.Value))
                Tokens.TryAdd(showroom.EmporiumId.Value, new CancellationTokenSource());

            return await _emporiumCache.GetOrSetAsync(
                $"listing:{showroom.Listings.First().Id.Value}",
                showroom,
                TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes),
                Tokens[showroom.EmporiumId.Value].Token);
        }

        public async ValueTask AddProcessingItemAsync(Listing listing)
        {
            if (!Tokens.ContainsKey(listing.Owner.EmporiumId.Value))
                Tokens.TryAdd(listing.Owner.EmporiumId.Value, new CancellationTokenSource());

            await _emporiumCache.SetAsync($"processing:{listing.Id.Value}", listing,
                                                     TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes),
                                                     Tokens[listing.Owner.EmporiumId.Value].Token);
            return;
        }

        public async ValueTask<Listing> GetProcessingItemAsync(Listing listing)
        {
            if (!Tokens.ContainsKey(listing.Owner.EmporiumId.Value))
                Tokens.TryAdd(listing.Owner.EmporiumId.Value, new CancellationTokenSource());

            return await _emporiumCache.GetOrDefaultAsync<Listing>($"processing:{listing.Id.Value}", token: Tokens[listing.Owner.EmporiumId.Value].Token);
        }
    }
}
