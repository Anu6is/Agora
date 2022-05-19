using Agora.Shared.Services;
using Emporia.Application.Common;
using Emporia.Application.Features.Commands;
using Emporia.Application.Features.Queries;
using Emporia.Application.Specifications;
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
            if (Tokens.TryRemove(guildId, out var source))
                source.Cancel();
        }

        public async ValueTask AddEmporiumAsync(Emporium emporium)
            => await _emporiumCache.SetAsync($"emporium:{emporium.Id.Value}",
                                             new EmporiumDetailsResponse() 
                                             { 
                                                 EmporiumId = emporium.Id.Value, 
                                                 Categories = emporium.Categories, 
                                                 Currencies = emporium.Currencies, 
                                                 Showrooms = emporium.Showrooms,
                                                 TimeOffset = emporium.TimeOffset
                                             },
                                             TimeSpan.FromMinutes(LongCacheExpirationInMinutes));

        public async ValueTask RemoveEmporiumAsync(ulong guildId)
            => await _emporiumCache.RemoveAsync($"emporium:{guildId}");
        
        public EmporiumDetailsResponse GetCachedEmporium(ulong guildId)
            => _emporiumCache.GetOrDefault<EmporiumDetailsResponse>($"emporium:{guildId}");

        public async ValueTask<EmporiumDetailsResponse> GetEmporiumAsync(ulong guildId)
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

                               return result.Data;
                           },
                           TimeSpan.FromMinutes(LongCacheExpirationInMinutes),
                           Tokens[guildId].Token);
        }

        public ValueTask RemoveUserAsync(ulong guildId, ulong userId)
            => _emporiumCache.RemoveAsync($"user:{guildId}:{userId}");

        public ValueTask<EmporiumUserDetailsResponse> GetUserAsync(ulong guildId, ulong userId)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());            
            
            return _emporiumCache.GetOrSetAsync(
                $"user:{guildId}:{userId}",
                async cts =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                    var result = await mediator.Send(new GetEmporiumUserDetailsQuery(new EmporiumId(guildId), ReferenceNumber.Create(userId)), cts);

                    if (result.Data == null)
                    {
                        var newUser = await mediator.Send(new CreateEmporiumUserCommand(new EmporiumId(guildId), ReferenceNumber.Create(userId)), cts);
                        var userDetails = new EmporiumUserDetailsResponse() 
                        { 
                            UserId = newUser.Id.Value, 
                            EmporiumId = newUser.EmporiumId.Value,
                            ReferenceNumber = newUser.ReferenceNumber.Value
                        };
                        
                        return userDetails;
                    }
                    
                    return result.Data;
                },
                TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes), 
                Tokens[guildId].Token);
        }

        public ValueTask RemoveShowroomAsync(ulong channelId, string itemType)
            => _emporiumCache.RemoveAsync($"showroom:{channelId}:{itemType}");

        public async ValueTask<ShowroomDetailsResponse> GetShowroomAsync(ulong guildId, ulong channelId, ListingType itemType)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());
            
            return await _emporiumCache.GetOrSetAsync(
                $"showroom:{channelId}:{itemType}",
                async cts =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var result = await mediator.Send(new GetShowroomDetailsQuery(new EmporiumId(guildId), new ShowroomId(channelId), itemType), cts);

                    return result.Data;
                },
                TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes), 
                Tokens[guildId].Token);
        }

        public async ValueTask<PagedResponse<ShowroomDetailsResponse>> GetShowroomsAsync(ulong guildId, int pageNumber = 1)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());
            
            return await _emporiumCache.GetOrSetAsync(
                           $"showrooms:{guildId}-{pageNumber}",
                           async cts =>
                           {
                               using var scope = _serviceProvider.CreateScope();
                               var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                               var filter = new ShowroomFilter(new EmporiumId(guildId)) { IsPagingEnabled = true, PageNumber = pageNumber };
                               var result = await mediator.Send(new GetPagedShowroomQuery(filter) { Cache = true }, cts);
                               return result;
                           },
                           TimeSpan.FromMinutes(ShortCacheExpirtionInMinutes),
                           Tokens[guildId].Token);
        }
    }
}
