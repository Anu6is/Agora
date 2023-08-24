using Emporia.Domain.Common;
using Emporia.Domain.Services;

namespace Agora.Shared.EconomyFactory
{
    public interface IEconomy
    {
        ValueTask<IResult<Money>> GetBalanceAsync(IEmporiumUser user, Currency currency);
        ValueTask<IResult> SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "");
        ValueTask<IResult> DeleteBalanceAsync(IEmporiumUser user, Currency currency, string reason = "");
        ValueTask<IResult<Money>> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "");
        ValueTask<IResult<Money>> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "");
    }
}
