using Agora.Shared.Services;
using Emporia.Application.Common;
using Emporia.Application.Features.Queries;
using Emporia.Application.Specifications;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Agora.Shared.Cache
{
    public class EporiaCacheService : AgoraService, IEmporiaCacheService
    {
        private const int CacheExpirationInMinutes = 15;
        
        private readonly IFusionCache _settingsCache;
        private readonly IServiceProvider _serviceProvider;

        public EporiaCacheService(IFusionCache cache, ILogger<IGuildSettingsService> logger, IServiceProvider services) : base(logger)
        {
            _settingsCache = cache;
            _serviceProvider = services;
        }

        public async ValueTask AddEmporiumAsync(Emporium emporium)
            => await _settingsCache.SetAsync($"emporium:{emporium.Id.Value}", emporium, TimeSpan.FromMinutes(CacheExpirationInMinutes));

        public EmporiumDetailsResponse GetCachedEmporium(ulong guildId)
            => _settingsCache.GetOrDefault<EmporiumDetailsResponse>($"emporium:{guildId}");

        public async ValueTask<EmporiumDetailsResponse> GetEmporiumAsync(ulong guildId)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await _settingsCache.GetOrSetAsync(
                           $"emporium:{guildId}",
                           async cts =>
                           {
                               var result = await mediator.Send(new GetEmporiumDetailsQuery(new EmporiumId(guildId)) { Cache = true }, cts);
                               return result.Data;
                           },
                           TimeSpan.FromMinutes(CacheExpirationInMinutes));
        }

        public async ValueTask RemoveEmporiumAsync(ulong guildId)
            => await _settingsCache.RemoveAsync($"emporium:{guildId}");

        public async ValueTask<PagedResponse<ShowroomDetailsResponse>> GetShowroomsAsync(ulong guildId)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await _settingsCache.GetOrSetAsync(
                           $"showrooms:{guildId}",
                           async cts =>
                           {
                               var filter = new ShowroomFilter(new EmporiumId(guildId)) { IsPagingEnabled = true };
                               var result = await mediator.Send(new GetShowroomListQuery(filter) { Cache = true }, cts);
                               return result;
                           },
                           TimeSpan.FromMinutes(CacheExpirationInMinutes));
        }
    }
}
