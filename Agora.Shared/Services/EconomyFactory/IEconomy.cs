using Emporia.Domain.Common;

namespace Agora.Shared.Services.EconomyFactory
{
    public interface IEconomy
    {
        ValueTask<Money> GetBalanceAsync(IEmporiumUser user, Currency currency);
        ValueTask SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "");
        ValueTask DeleteBalanceAsync(IEmporiumUser user, Currency currency, string reason = "");
        ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "");
        ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "");
    }
}
