using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Emporia.Domain.Common;
using Emporia.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
    public class EconomyService : AgoraService, IEconomy
    {
        public EconomyService(ILogger<EconomyService> logger) : base(logger) { }

        public virtual ValueTask<IResult<Money>> GetBalanceAsync(IEmporiumUser user, Currency currency) => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask<IResult> SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "") => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask<IResult> DeleteBalanceAsync(IEmporiumUser user, Currency currency, string reason = "") => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask<IResult<Money>> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "") => throw new NotImplementedException("Economy is not enabled.");

        public virtual ValueTask<IResult<Money>> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "") => throw new NotImplementedException("Economy is not enabled.");
    }
}
