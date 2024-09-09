using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory.Providers;


[AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
public class AgoraEconomyProvider : AgoraService, IEconomyProvider
{
    public AgoraEconomyProvider(ILogger<AgoraEconomyProvider> logger) : base(logger) { }

    public string EconomyType => "AuctionBot";

    public IEconomy CreateEconomy(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<AgoraEconomy>();
    }
}
