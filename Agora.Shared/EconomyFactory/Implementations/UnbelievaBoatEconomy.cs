using Agora.Shared.Attributes;
using Believe.Net;
using Emporia.Domain.Common;
using FluentValidation;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
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

            return Money.Create(ParseToDecimal(userBalance.Cash < 0 ? userBalance.Total : userBalance.Bank), currency);
        }

        public override async ValueTask SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var userBalance = await _unbelievaClient.SetUserBankAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason);

            if (userBalance.IsRateLimited) throw new RateLimitException($"UnbelievaBoat transaction processing is on cooldown. Retry after {userBalance.RetryAfter.Humanize()}");

            return;
        }

        public override async ValueTask DeleteBalanceAsync(IEmporiumUser user, Currency currency, string reason = "")
        {
            var userBalance = await _unbelievaClient.SetUserBankAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, 0, reason);

            if (userBalance.IsRateLimited) throw new RateLimitException($"UnbelievaBoat transaction processing is on cooldown. Retry after {userBalance.RetryAfter.Humanize()}");

            return;
        }

        public override async ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var userBalance = await _unbelievaClient.IncreaseUserBankAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason);

            if (userBalance == null)
                await CheckEconomyAccess(user);

            if (userBalance.IsRateLimited)
            {
                await Task.Delay(userBalance.RetryAfter);
                await IncreaseBalanceAsync(user, amount, reason);
            }

            return Money.Create(ParseToDecimal(userBalance.Total), amount.Currency);
        }

        public override async ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var userBalance = await _unbelievaClient.DecreaseUserBankAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason);

            if (userBalance == null)
                await CheckEconomyAccess(user);

            if (userBalance.IsRateLimited) throw new RateLimitException($"UnbelievaBoat transaction processing is on cooldown. Retry after {userBalance.RetryAfter.Humanize()}");

            return Money.Create(ParseToDecimal(userBalance.Total), amount.Currency);
        }

        private async ValueTask CheckEconomyAccess(IEmporiumUser user)
        {
            var economyAccess = await _unbelievaClient.HasPermissionAsync(user.EmporiumId.Value, ApplicationPermission.EditEconomy);

            if (!economyAccess)
                throw new UnauthorizedAccessException("Auction Bot needs to be authorized to use UnbelivaBoat economy in this server!");

            throw new UnauthorizedAccessException("Unable to validate user's UnbelievaBoat balance");
        }

        private static decimal ParseToDecimal(double value)
        {
            if (double.IsInfinity(value)) return decimal.MaxValue;
            if (double.IsNaN(value)) return decimal.MinValue;

            try
            {
                return Convert.ToDecimal(value);
            }
            catch (Exception)
            {
                if (value > 0)
                    return decimal.MaxValue;
                else
                    return decimal.MinValue;
            }
        }
    }
}
