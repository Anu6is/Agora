using Agora.Shared.Attributes;
using Emporia.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.Services.EconomyFactory
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
    public class EconomyService : AgoraService, IEconomy
    {
        public EconomyService(ILogger<EconomyService> logger) : base(logger) { }

        public virtual ValueTask<Money> GetBalanceAsync(IEmporiumUser user, Currency currency) => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask SetBalanceAsync(IEmporiumUser user, Money amount) => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask DeleteBalanceAsync(IEmporiumUser user, Currency currency) => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount) => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount) => throw new NotImplementedException("Economy is not enabled.");
    }
}
