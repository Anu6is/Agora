using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory;

[AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
public class EconomyFactoryService : AgoraService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Func<IEconomy>> _economyFactories;

    public EconomyFactoryService(IServiceProvider services, IEnumerable<IEconomyProvider> economyProviders, ILogger<EconomyFactoryService> logger) : base(logger)
    {
        _serviceProvider = services;
        _economyFactories = new Dictionary<string, Func<IEconomy>>();

        foreach (var provider in economyProviders)
        {
            _economyFactories[provider.EconomyType] = () => provider.CreateEconomy(_serviceProvider);
        }
    }

    public IEconomy Create(string economyType = "Disabled")
    {
        if (_economyFactories.TryGetValue(economyType, out var factory))
        {
            return factory();
        }

        throw new NotImplementedException($"No implementation exists for {economyType}");
    }
}
