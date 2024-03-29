﻿using Agora.Shared.EconomyFactory;
using Agora.Shared.Extensions;
using Agora.Shared.Features.Commands;
using Agora.Shared.Persistence.Models;
using Emporia.Domain.Entities;
using Emporia.Domain.Events;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

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
                await economy.DecreaseBalanceAsync(economyUser, bid.Amount, $"Submitted bid for {notification.Listing.Product.Quantity} {notification.Listing.Product.Title}");
            else if (notification.Listing is CommissionTrade trade)
                await economy.DecreaseBalanceAsync(notification.Listing.Owner, trade.Commission, $"Paid a commission for {trade.Product.Title}");
            else if (notification.Listing is RaffleGiveaway raffle && raffle.Product is GiveawayItem item)
                await economy.DecreaseBalanceAsync(economyUser, item.TicketPrice, $"Purchased raffle ticket for {raffle.Product.Title}");

            return;
        }
    }

    internal class OfferAddedAlert : INotificationHandler<OfferAddedEvent>
    {
        private readonly IUserProfileService _profileService;
        private readonly IServiceScopeFactory _scopeFactory;

        public OfferAddedAlert(IUserProfileService profileService, IServiceScopeFactory scopeFactory)
        {
            _profileService = profileService;
            _scopeFactory = scopeFactory;
        }

        public async Task Handle(OfferAddedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Listing.Product is not AuctionItem item) return;
            if (notification.Listing is VickreyAuction) return;
            if (item.Offers.Count == 1) return;

            var previousBid = item.Offers.OrderByDescending(x => x.SubmittedOn).Skip(1).First();

            if (notification.Offer.UserReference == previousBid.UserReference) return;

            var emporiumId = notification.Listing.Owner.EmporiumId.Value;
            var profile = (UserProfile)await _profileService.GetUserProfileAsync(emporiumId, previousBid.UserReference.Value);

            if (!profile.OutbidAlerts) return;

            using var scope = _scopeFactory.CreateScope();

            var channelReference = notification.Listing.ReferenceCode.Reference();
            var channelId = channelReference == 0 ? notification.Listing.ShowroomId.Value : channelReference;

            var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
            var link = messageService.GetMessageUrl(emporiumId, channelId, item.ReferenceNumber.Value);
            var reference = $"*reference code:* [{notification.Listing.ReferenceCode.Code()}]({link})";

            var id = await messageService.SendDirectMessageAsync(profile.UserReference.Value, $"You have been outbid for **{item.Title}**\n{reference}");

            if (id == 0)
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new UpdateUserProfileCommand(profile.SetOutbidNotifications(false)), cancellationToken);
            }
        }
    }
}
