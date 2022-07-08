using Emporia.Domain.Common;

namespace Agora.Shared.Services.EconomyFactory
{
    public interface IEconomy
    {
        ValueTask SetBalanceAsync(IEmporiumUser user, Money amount);
        ValueTask DeleteBalanceAsync(IEmporiumUser user, Currency currency);
        ValueTask<Money> GetBalanceAsync(IEmporiumUser user, Currency currency);
        ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount);
        ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount);
    }
}
