using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory.Providers;

[AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
public class DisabledEconomyProvider : AgoraService, IEconomyProvider
{
    public DisabledEconomyProvider(ILogger<DisabledEconomyProvider> logger) : base(logger) { }

    public string EconomyType => "Disabled";

    public IEconomy CreateEconomy(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<PseudoEconomy>();
    }
}
