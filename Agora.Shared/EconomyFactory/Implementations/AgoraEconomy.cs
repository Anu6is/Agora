using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using Emporia.Persistence.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    public class AgoraEconomy : EconomyService
    {
        private readonly IDataAccessor _dataAccessor;
        private readonly IGuildSettingsService _guildSettingsService;

        public AgoraEconomy(IGuildSettingsService settingsService, IServiceScopeFactory scopeFactory, ILogger<AgoraEconomy> logger) : base(logger)
        {
            _dataAccessor = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDataAccessor>();
            _guildSettingsService = settingsService;
        }

        public override async ValueTask<Money> GetBalanceAsync(IEmporiumUser user, Currency currency)
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            return Money.Create(economyUser.Balance, currency);
        }

        public override async ValueTask SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(amount.Value);

            await _dataAccessor.CommitAsync();

            return;
        }

        public override async ValueTask DeleteBalanceAsync(IEmporiumUser user, Currency currency, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(0);

            await _dataAccessor.CommitAsync();

            return;
        }

        public override async ValueTask<Money> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(economyUser.Balance + amount.Value);

            await _dataAccessor.CommitAsync();

            return Money.Create(economyUser.Balance, amount.Currency);
        }

        public override async ValueTask<Money> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(economyUser.Balance - amount.Value);

            await _dataAccessor.CommitAsync();

            return Money.Create(economyUser.Balance, amount.Currency);
        }

        private async Task<DefaultEconomyUser> GetOrCreateEconomyUser(IEmporiumUser user)
        {
            var member = await _dataAccessor.Transaction<GenericRepository<DefaultEconomyUser>>().GetByIdAsync(user.Id);

            if (member == null)
            {
                var settings = await _guildSettingsService.GetGuildSettingsAsync(user.EmporiumId.Value);

                member = DefaultEconomyUser.FromEmporiumUser(user).WithBalance(settings.DefaultBalance);

                _dataAccessor.Create(member);
            }

            return member;
        }
    }
}
