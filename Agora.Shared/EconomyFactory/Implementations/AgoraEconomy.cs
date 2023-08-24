using Agora.Shared.Attributes;
using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Domain.Services;
using Emporia.Extensions.Discord;
using Emporia.Persistence.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Shared.EconomyFactory
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Transient)]
    public class AgoraEconomy : EconomyService
    {
        private readonly IDataAccessor _dataAccessor;
        private readonly IGuildSettingsService _guildSettingsService;

        public AgoraEconomy(IGuildSettingsService settingsService, IServiceScopeFactory scopeFactory, ILogger<AgoraEconomy> logger) : base(logger)
        {
            _dataAccessor = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDataAccessor>();
            _guildSettingsService = settingsService;
        }

        public override async ValueTask<IResult<Money>> GetBalanceAsync(IEmporiumUser user, Currency currency)
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            return Result.Success(Money.Create(economyUser.Balance, currency));
        }

        public override async ValueTask<IResult> SetBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(amount.Value);

            var result = await _dataAccessor.CommitAsync();

            if (!result.IsSuccessful) return Result.Failure("Error attempting to set user balance");

            return Result.Success();
        }

        public override async ValueTask<IResult> DeleteBalanceAsync(IEmporiumUser user, Currency currency, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(0);

            var result = await _dataAccessor.CommitAsync();

            if (!result.IsSuccessful) return Result.Failure("Error attempting to delete user balance");

            return Result.Success();
        }

        public override async ValueTask<IResult<Money>> IncreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(economyUser.Balance + amount.Value);

            var result = await _dataAccessor.CommitAsync();

            if (!result.IsSuccessful) return Result<Money>.Failure("Error attempting to increase user balance");

            return Result.Success(Money.Create(economyUser.Balance, amount.Currency));
        }

        public override async ValueTask<IResult<Money>> DecreaseBalanceAsync(IEmporiumUser user, Money amount, string reason = "")
        {
            var economyUser = await GetOrCreateEconomyUser(user);

            economyUser.WithBalance(economyUser.Balance - amount.Value);

            var result = await _dataAccessor.CommitAsync();

            if (!result.IsSuccessful) return Result<Money>.Failure("Error attempting to decrease user balance");

            return Result.Success(Money.Create(economyUser.Balance, amount.Currency));
        }

        private async Task<DefaultEconomyUser> GetOrCreateEconomyUser(IEmporiumUser user)
        {
            var member = await _dataAccessor.Transaction<GenericRepository<DefaultEconomyUser>>().GetByIdAsync(user.Id);

            if (member == null)
            {
                var settings = await _guildSettingsService.GetGuildSettingsAsync(user.EmporiumId.Value);

                member = DefaultEconomyUser.FromEmporiumUser(user).WithBalance(settings.DefaultBalance);

                _dataAccessor.Create(member);

                await _dataAccessor.CommitAsync();
            }

            return member;
        }
    }
}
