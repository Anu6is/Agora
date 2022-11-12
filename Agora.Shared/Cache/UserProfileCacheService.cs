using Agora.Shared.Features.Commands;
using Agora.Shared.Features.Queries;
using Agora.Shared.Persistence.Models;
using Agora.Shared.Services;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ZiggyCreatures.Caching.Fusion;

namespace Agora.Shared.Cache
{
    public class UserProfileCacheService : AgoraService, IUserProfileService
    {
        private const int CacheExpirationInMinutes = 5;

        private readonly IFusionCache _profileCache;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> Tokens;

        public UserProfileCacheService(IFusionCache cache, ILogger<IUserProfileService> logger, IServiceScopeFactory factory) : base(logger)
        {
            Tokens = new();
            _profileCache = cache;
            _scopeFactory = factory;
        }

        public async ValueTask AddUserProfileAsync(IUserProfile profile)
        {
            if (!Tokens.ContainsKey(profile.EmporiumId.Value))
                Tokens.TryAdd(profile.EmporiumId.Value, new CancellationTokenSource());

            await _profileCache.SetAsync($"userprofile:{profile.EmporiumId.Value}:{profile.UserReference.Value}",
                                         profile,
                                         TimeSpan.FromMinutes(CacheExpirationInMinutes),
                                         Tokens[profile.EmporiumId.Value].Token);
        }

        public async ValueTask<IUserProfile> GetUserProfileAsync(ulong guildId, ulong userReference)
        {
            if (!Tokens.ContainsKey(guildId))
                Tokens.TryAdd(guildId, new CancellationTokenSource());

            var emporiumId = new EmporiumId(guildId);
            var reference = ReferenceNumber.Create(userReference);

            return await _profileCache.GetOrSetAsync<IUserProfile>(
                $"userprofile:{guildId}:{userReference}",
                async cts =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var result = await mediator.Send(new GetUserProfileQuery(emporiumId, reference), cts);

                    if (result.Data == null)
                    {
                        var user = await scope.ServiceProvider.GetRequiredService<IEmporiaCacheService>().GetUserAsync(guildId, userReference);
                        var profile = await mediator.Send(new CreateUserProfileCommand(new EmporiumId(user.EmporiumId), new UserId(user.UserId), ReferenceNumber.Create(user.ReferenceNumber)), cts);

                        return profile;
                    }
                    var data = result.Data;

                    return UserProfile.Create(new EmporiumId(data.EmporiumId), new UserId(data.UserId), ReferenceNumber.Create(data.ReferenceNumber))
                                      .SetTradeDealNotifications(data.TradeDealAlerts)
                                      .SetOutbidNotifications(data.OutbidAlerts)
                                      .SetReviewCount(data.Reviews)
                                      .SetRating(data.Rating);
                },
                TimeSpan.FromMinutes(CacheExpirationInMinutes),
                Tokens[guildId].Token);
        }

        public void Clear(ulong guildId, ulong userReference)
        {
            _profileCache.Remove($"userprofile:{guildId}:{userReference}");

            if (Tokens.TryRemove(guildId, out var source))
                source.Cancel();
        }
    }
}
