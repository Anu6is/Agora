using Agora.Shared.EconomyFactory;
using Emporia.Domain.Entities;
using Emporia.Domain.Events;
using Emporia.Extensions.Discord;
using MediatR;

namespace Agora.Shared.Events
{
    internal class OnOfferAdded : INotificationHandler<OfferAddedEvent>
    {
        private readonly IGuildSettingsService _guildSettingsService;
        private readonly IEmporiaCacheService _emporiaCache;
        private readonly EconomyFactoryService _factory;

        public OnOfferAdded(IGuildSettingsService guildSettingsService, IEmporiaCacheService cache, EconomyFactoryService factory)
        {
            _guildSettingsService = guildSettingsService;
            _emporiaCache = cache;
            _factory = factory;
        }

        public async Task Handle(OfferAddedEvent notification, CancellationToken cancellationToken)
        {
            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(notification.Listing.Owner.EmporiumId.Value);

            if (guildSettings.EconomyType == "Disabled") return;

            var user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, notification.Offer.UserReference.Value);
            var economy = _factory.Create(guildSettings.EconomyType);
            var economyUser = user.ToEmporiumUser();

            if (notification.Offer is Payment payment)
                await economy.DecreaseBalanceAsync(economyUser, payment.Amount, $"Purchased {payment.ItemCount} {notification.Listing.Product.Title}");
            else if (notification.Offer is Bid bid)
            {
                await economy.DecreaseBalanceAsync(economyUser, bid.Amount, $"Submitted bid for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");

                if (notification.Listing is VickreyAuction) return;

                var item = notification.Listing.Product as AuctionItem;

                if (item.Offers.Count <= 1) return;

                var previousBid = item.Offers.OrderByDescending(x => x.SubmittedOn).Skip(1).First();
                user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, previousBid.UserReference.Value);

                await economy.IncreaseBalanceAsync(user.ToEmporiumUser(), previousBid.Amount, $"Bid returned for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");
            }

            return;
        }
    }
}
