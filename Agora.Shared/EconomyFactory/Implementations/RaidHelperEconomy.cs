using Agora.Shared.Attributes;
using Emporia.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
    public class RaidHelperEconomy : EconomyService 
    {
        private readonly RaidHelperClient _raidHelperClient;

        public RaidHelperEconomy(RaidHelperClient client, ILogger<RaidHelperEconomy> logger) : base(logger) 
        {
            _raidHelperClient = client;
        }

        public override async ValueTask<Money> GetBalanceAsync(IEmporiumUser user, Currency currency)
        {
            var entity = await _raidHelperClient.GetUserBalanceAsync(user.EmporiumId.Value, user.ReferenceNumber.Value) 
                ?? throw new ValidationException("Unable to verify DKP balance");
            
            _ = decimal.TryParse(entity.Dkp, out var dkp);

            return Money.Create(dkp, currency);
        }

        public override async ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var entity = await _raidHelperClient.IncreaseUserBalanceAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason) 
                ?? throw new ValidationException("Failed to increase DKP balance");

            _ = decimal.TryParse(entity.Dkp, out var dkp);

            return Money.Create(dkp, amount.Currency);
        }

        public override async ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var entity = await _raidHelperClient.DecreaseUserBalanceAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason)
                ?? throw new ValidationException("Failed to decrease DKP balance");

            _ = decimal.TryParse(entity.Dkp, out var dkp);

            return Money.Create(dkp, amount.Currency);
        }

        public override async ValueTask SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            _ = await _raidHelperClient.SetUserBalanceAsync(user.EmporiumId.Value, user.ReferenceNumber.Value, amount.Value, reason)
                ?? throw new ValidationException("Failed to set DKP balance");
        }
    }
}
