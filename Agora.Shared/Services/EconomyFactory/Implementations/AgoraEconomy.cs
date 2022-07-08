using Microsoft.Extensions.Logging;

namespace Agora.Shared.Services.EconomyFactory
{
    public class AgoraEconomy : EconomyService
    {
        public AgoraEconomy(ILogger<AgoraEconomy> logger) : base(logger)
        {
        }
    }
}
