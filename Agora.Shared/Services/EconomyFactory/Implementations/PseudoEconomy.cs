using Emporia.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.Services.EconomyFactory
{
    public class PseudoEconomy : EconomyService
    {
        public PseudoEconomy(ILogger<PseudoEconomy> logger) : base(logger) { }

        public override ValueTask<Money> GetBalanceAsync(IEmporiumUser user, Currency currency) => ValueTask.FromResult(Money.Create(decimal.MaxValue, currency));

        public override ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount) => ValueTask.FromResult(Money.Create(decimal.MaxValue, amount.Currency));

        public override ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount) => ValueTask.FromResult(Money.Create(decimal.MaxValue, amount.Currency));
    }
}
