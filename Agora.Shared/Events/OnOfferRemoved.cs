﻿using Agora.Shared.EconomyFactory;
using Emporia.Domain.Entities;
using Emporia.Domain.Events;
using Emporia.Extensions.Discord;
using MediatR;

namespace Agora.Shared.Events
{
    internal class OnOfferRemoved : INotificationHandler<OfferRemovedEvent>
    {
        private readonly IGuildSettingsService _guildSettingsService;
        private readonly IEmporiaCacheService _emporiaCache;
        private readonly EconomyFactoryService _factory;

        public OnOfferRemoved(IGuildSettingsService guildSettingsService, IEmporiaCacheService cache, EconomyFactoryService factory)
        {
            _guildSettingsService = guildSettingsService;
            _emporiaCache = cache;
            _factory = factory;
        }

        public async Task Handle(OfferRemovedEvent notification, CancellationToken cancellationToken)
        {
            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(notification.Listing.Owner.EmporiumId.Value);

            if (guildSettings.EconomyType == "Disabled") return;

            var user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, notification.Offer.UserReference.Value);
            var economy = _factory.Create(guildSettings.EconomyType);
            var economyUser = user.ToEmporiumUser();

            if (notification.Offer is Bid bid)
            {
                await economy.IncreaseBalanceAsync(economyUser, bid.Amount, $"Recalled bid for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");

                if (notification.Listing is VickreyAuction) return;

                var item = notification.Listing.Product as AuctionItem;

                if (item.Offers.Count == 0) return;

                var previousBid = item.Offers.OrderByDescending(x => x.SubmittedOn).First();
                user = await _emporiaCache.GetUserAsync(notification.Listing.Owner.EmporiumId.Value, previousBid.UserReference.Value);

                await economy.DecreaseBalanceAsync(user.ToEmporiumUser(), previousBid.Amount, $"Resubmitted bid for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");
            }

            return;
        }
    }
}