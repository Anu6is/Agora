using Believe.Net;
using Emporia.Domain.Common;
using FluentValidation;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    public class UnbelievaBoatEconomy : EconomyService
    {
        private readonly UnbelievaClient _unbelievaClient;

        public UnbelievaBoatEconomy(UnbelievaClient client, ILogger<UnbelievaBoatEconomy> logger) : base(logger)
        {
            _unbelievaClient = client;
        }

        public override async ValueTask<Money> GetBalanceAsync(IEmporiumUser user, Currency currency)
        {
            var userBalance = await _unbelievaClient.GetUserBalanceAsync(user.EmporiumId.Value, user.ReferenceNumber.Value);

            if (userBalance == null) throw new ValidationException("Unable to verify user balance");
            if (userBalance.IsRateLimited) throw new RateLimitException($"UnbelievaBoat transaction processing is on cooldown. Retry after {userBalance.RetryAfter.Humanize()}");

            return Money.Create((decimal)userBalance.Total, currency);
        }

        public override async ValueTask SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var userBalance = await _unbelievaClient.SetUserBalanceAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, 0, amount.Value, reason);

            if (userBalance.IsRateLimited) throw new RateLimitException($"UnbelievaBoat transaction processing is on cooldown. Retry after {userBalance.RetryAfter.Humanize()}");

            return;
        }

        public override async ValueTask DeleteBalanceAsync(IEmporiumUser user, Currency currency, string reason = "")
        {
            var userBalance = await _unbelievaClient.SetUserBalanceAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, 0, 0, reason);

            if (userBalance.IsRateLimited) throw new RateLimitException($"UnbelievaBoat transaction processing is on cooldown. Retry after {userBalance.RetryAfter.Humanize()}");

            return;
        }

        public override async ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var userBalance = await _unbelievaClient.IncreaseUserBankAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason);

            if (userBalance == null) await CheckEconomyAccess(user);

            if (userBalance.IsRateLimited)
            {
                await Task.Delay(userBalance.RetryAfter);
                await IncreaseBalanceAsync(user, amount, reason);
            }

            return Money.Create((decimal)userBalance.Total, amount.Currency);
        }

        public override async ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var userBalance = await _unbelievaClient.DecreaseUserCashAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason);

            if (userBalance == null) await CheckEconomyAccess(user);

            if (userBalance.IsRateLimited) throw new RateLimitException($"UnbelievaBoat transaction processing is on cooldown. Retry after {userBalance.RetryAfter.Humanize()}");

            return Money.Create((decimal)userBalance.Total, amount.Currency);
        }

        private async ValueTask CheckEconomyAccess(IEmporiumUser user)
        {
            var economyAccess = await _unbelievaClient.HasPermissionAsync(user.EmporiumId.Value, ApplicationPermission.EditEconomy);

            if (!economyAccess)
                throw new UnauthorizedAccessException("Auction Bot needs to be authorized to use UnbelivaBoat economy in this server!");

            throw new NullReferenceException("An error occurred while attempting to access the UnbelievaBoat balance");
        }
    }
}
