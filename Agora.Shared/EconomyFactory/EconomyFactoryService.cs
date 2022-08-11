using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    public enum EconomyType { Disabled, AuctionBot, UnbelievaBoat }

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
            _ => throw new NotImplementedException($"No implementation exists for {economyType}")
        };
    }
}
