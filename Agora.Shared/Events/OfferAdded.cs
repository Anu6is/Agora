using Agora.Shared.EconomyFactory;
using Emporia.Application.Common;
using Emporia.Application.Specifications;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Domain.Events;
using Emporia.Extensions.Discord;
using MediatR;

namespace Agora.Shared.Events
{
    internal class OfferAdded : INotificationHandler<OfferAddedNotification>
    {
        private readonly IGuildSettingsService _guildSettingsService;
        private readonly IEmporiaCacheService _emporiaCache;
        private readonly IDataAccessor _dataAccessor;
        private readonly EconomyFactoryService _factory;

        public OfferAdded(IGuildSettingsService guildSettingsService, IEmporiaCacheService cache, IDataAccessor dataAccessor, EconomyFactoryService factory)
        {
            _guildSettingsService = guildSettingsService;
            _dataAccessor = dataAccessor;
            _emporiaCache = cache;
            _factory = factory;
        }

        public async Task Handle(OfferAddedNotification notification, CancellationToken cancellationToken)
        {
            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(notification.Listing.Owner.EmporiumId.Value);

            if (guildSettings.EconomyType == "Disabled") return;
            if (notification.Listing is VickreyAuction) return;
            if (notification.Listing is FlashMarket) return;

            var economy = _factory.Create(guildSettings.EconomyType);

            var filter = new ShowroomFilter(notification.Listing.Owner.EmporiumId, notification.Listing.ShowroomId, notification.Listing.Type.ToString())
            {
                ListingId = notification.Listing.Id,
            };

            var showroom = await _dataAccessor.Transaction<IReadRepository<Showroom>>().SingleOrDefaultAsync(new ShowroomSpec(filter), cancellationToken);
            var listing = showroom.Listings.FirstOrDefault();

            if (listing?.Product is AuctionItem auction && auction.Offers.Count > 1) 
                await ReturnPreviousBidAsync(notification, auction, economy);
            if (listing is StandardMarket { AllowOffers: true })
                await ReturnPreviousOfferAsync(listing.Owner.EmporiumId, (MarketItem)listing.Product, economy);
            else if (listing is MassMarket or MultiItemMarket)
                await PartialPurchaseAsync(notification, (MarketItem)listing.Product, economy);
            else if (notification.Listing is CommissionTrade trade)
                await SellItemAsync(notification, trade, economy);
            return;
        }

        private async Task SellItemAsync(OfferAddedNotification notification, CommissionTrade trade, IEconomy economy)
        {
            var user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, notification.Offer.UserReference.Value);

            await economy.IncreaseBalanceAsync(user.ToEmporiumUser(), trade.Commission, $"Commission for {trade.Product.Title}");
        }

        private async Task ReturnPreviousOfferAsync(EmporiumId emporiumId, MarketItem item, IEconomy economy)
        {
            if (item.Offers.Count <= 1) return;

            var offers = item.Offers.OrderByDescending(x => x.SubmittedOn);

            var previousOffer = offers.Skip(1).First();
            var refundee = await _emporiaCache.GetUserAsync(emporiumId.Value, previousOffer.UserReference.Value);

            await economy.IncreaseBalanceAsync(refundee.ToEmporiumUser(), previousOffer.Amount, $"Offer returned for {item.Quantity} {item.Title}");
        }

        private async Task PartialPurchaseAsync(OfferAddedNotification notification, MarketItem market, IEconomy economy)
        {
            if (notification.Listing.Status >= ListingStatus.Withdrawn) return;
            if (notification.Offer is not Payment payment) return;

            await economy.IncreaseBalanceAsync(notification.Listing.Owner, payment.Amount, $"Partial purchase of {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");
        }

        private async Task ReturnPreviousBidAsync(OfferAddedNotification notification, AuctionItem auction, IEconomy economy)
        {
            var previousBid = auction.Offers.OrderByDescending(x => x.SubmittedOn).First(x => x.SubmittedOn < notification.Offer.SubmittedOn);

            var user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, previousBid.UserReference.Value);

            await economy.IncreaseBalanceAsync(user.ToEmporiumUser(), previousBid.Amount, $"Bid returned for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");
        }
    }
}
