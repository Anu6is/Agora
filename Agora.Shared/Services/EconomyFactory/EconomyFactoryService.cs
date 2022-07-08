using Agora.Shared.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.Services.EconomyFactory
{
    public enum EconomyType { None, Agora, UnbelievaBoat }

    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
    public class EconomyFactoryService : AgoraService
    {
        private readonly IServiceProvider _serviceProvider;
        public EconomyFactoryService(ILogger<EconomyFactoryService> logger, IServiceProvider services) : base(logger)
        {
            _serviceProvider = services;
        }

        public IEconomy Create(string economyType = "None") => economyType switch
        {
            nameof(EconomyType.None) => _serviceProvider.GetRequiredService<PseudoEconomy>(),
            nameof(EconomyType.Agora) => _serviceProvider.GetRequiredService<AgoraEconomy>(),
            nameof(EconomyType.UnbelievaBoat) => _serviceProvider.GetRequiredService<UnbelievaBoatEconomy>(),
            _ => throw new NotImplementedException($"No implementation exists for {economyType}")
        };
    }
}
