using Agora.Shared.Attributes;
using Emporia.Domain.Common;
using Emporia.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
    public class PseudoEconomy : EconomyService
    {
        public PseudoEconomy(ILogger<PseudoEconomy> logger) : base(logger) { }

        public override ValueTask<IResult<Money>> GetBalanceAsync(IEmporiumUser user, Currency currency) => Result.Success(Money.Create(decimal.MaxValue, currency));

        public override ValueTask<IResult<Money>> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "") => Result.Success(Money.Create(decimal.MaxValue, amount.Currency));

        public override ValueTask<IResult<Money>> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "") => Result.Success(Money.Create(decimal.MaxValue, amount.Currency));
    }
}
