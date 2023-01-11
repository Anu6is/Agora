using Agora.Shared.EconomyFactory;
using Emporia.Application.Common;
using Emporia.Application.Specifications;
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
            if (notification.Listing.Product is not AuctionItem item) return;

            var user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, notification.Offer.UserReference.Value);
            var economy = _factory.Create(guildSettings.EconomyType);
            var economyUser = user.ToEmporiumUser();

            var filter = new ShowroomFilter(notification.Listing.Owner.EmporiumId, notification.Listing.ShowroomId, notification.Listing.Type.ToString())
            {
                ListingId = notification.Listing.Id,
            };

            var showroom = await _dataAccessor.Transaction<IReadRepository<Showroom>>().SingleOrDefaultAsync(new ShowroomSpec(filter), cancellationToken);

            if (showroom.Listings.FirstOrDefault()?.Product is not AuctionItem auction || auction.Offers.Count <= 1) return;

            var previousBid = auction.Offers.OrderByDescending(x => x.SubmittedOn).First(x => x.SubmittedOn < notification.Offer.SubmittedOn);

            user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, previousBid.UserReference.Value);

            await economy.IncreaseBalanceAsync(user.ToEmporiumUser(), previousBid.Amount, $"Bid returned for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");


            return;
        }
    }
}
