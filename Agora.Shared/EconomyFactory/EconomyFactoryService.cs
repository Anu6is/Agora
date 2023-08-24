using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    public enum EconomyType { Disabled, AuctionBot, UnbelievaBoat, RaidHelper }

    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
    public class EconomyFactoryService : AgoraService
    {
        private readonly IServiceProvider _serviceProvider;
        public EconomyFactoryService(IServiceProvider services, ILogger<EconomyFactoryService> logger) : base(logger)
        {
            _serviceProvider = services;
        }

        public IEconomy Create(string economyType = "Disabled") => economyType switch
        {
            nameof(EconomyType.Disabled) => _serviceProvider.GetRequiredService<PseudoEconomy>(),
            nameof(EconomyType.AuctionBot) => _serviceProvider.GetRequiredService<AgoraEconomy>(),
            nameof(EconomyType.UnbelievaBoat) => _serviceProvider.GetRequiredService<UnbelievaBoatEconomy>(),
            nameof(EconomyType.RaidHelper) => _serviceProvider.GetRequiredService<RaidHelperEconomy>(),
            _ => throw new NotImplementedException($"{economyType} is not a supported economy interface")
        };
    }
}
