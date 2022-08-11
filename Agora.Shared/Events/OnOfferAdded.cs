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
        private readonly EconomyFactoryService _factory;

        public OnOfferAdded(IGuildSettingsService guildSettingsService, EconomyFactoryService factory)
        {
            _guildSettingsService = guildSettingsService;
            _factory = factory;
        }

        public async Task Handle(OfferAddedEvent notification, CancellationToken cancellationToken)
        {
            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(notification.Listing.Owner.EmporiumId.Value);

            if (guildSettings.EconomyType == "Disabled") return;

            var user = EmporiumUser.Create(notification.Listing.Owner.EmporiumId, notification.Offer.UserReference);

            var economy = _factory.Create(guildSettings.EconomyType);

            if (notification.Offer is Payment payment)
                await economy.DecreaseBalanceAsync(user, payment.Amount, $"Purchased {payment.ItemCount} {notification.Listing.Product.Title}");
            else if (notification.Offer is Bid bid)
            {
                await economy.DecreaseBalanceAsync(user, bid.Amount, $"Submitted bid for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");

                if (notification.Listing is VickreyAuction) return;

                var item = notification.Listing.Product as AuctionItem;

                if (item.Offers.Count <= 1) return;

                var previousBid = item.Offers.OrderByDescending(x => x.SubmittedOn).Skip(1).First();
                user = EmporiumUser.Create(notification.Listing.Owner.EmporiumId, previousBid.UserReference);

                await economy.IncreaseBalanceAsync(user, previousBid.Amount, $"Bid returned for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");
            }

            return;
        }
    }
}
