using Agora.Shared.Cache;
using Agora.Shared.EconomyFactory;
using Emporia.Application.Common;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Domain.Events;
using Emporia.Extensions.Discord;
using MediatR;

namespace Agora.Shared.Events
{
    internal class ListingRemovedSeller : INotificationHandler<ListingRemovedNotification>
    {
        private readonly IGuildSettingsService _guildSettingsService;
        private readonly EconomyFactoryService _factory;

        public ListingRemovedSeller(IGuildSettingsService guildSettingsService, EconomyFactoryService factory)
        {
            _guildSettingsService = guildSettingsService;
            _factory = factory;
        }

        public async Task Handle(ListingRemovedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.ProductListing.Status != ListingStatus.Sold) return;
            if (notification.ProductListing.Product is not AuctionItem and not MarketItem) return;

            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(notification.ProductListing.Owner.EmporiumId.Value);

            if (guildSettings.EconomyType == "Disabled") return;

            var product = notification.ProductListing.Product;
            var economy = _factory.Create(guildSettings.EconomyType);
            var submission = product switch
            {
                AuctionItem auction => auction.Offers.OrderByDescending(x => x.SubmittedOn).First().Amount,
                MarketItem market => market.Offers.OrderByDescending(x => x.SubmittedOn).First().Amount,
                _ => throw new InvalidOperationException($"Cannot increase balances for {product.GetType()}")
            };

            await economy.IncreaseBalanceAsync(notification.ProductListing.Owner, submission, $"Sale of {notification.ProductListing.Product.Title}");

            return;
        }
    }

    internal class ListingRemovedBuyer : INotificationHandler<ListingRemovedNotification>
    {
        private readonly IGuildSettingsService _guildSettingsService;
        private readonly EconomyFactoryService _factory;

        public ListingRemovedBuyer(IGuildSettingsService guildSettingsService, EconomyFactoryService factory)
        {
            _guildSettingsService = guildSettingsService;
            _factory = factory;
        }

        public async Task Handle(ListingRemovedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.ProductListing.Product is not AuctionItem item) return;

            var emporiumId = notification.ProductListing.Owner.EmporiumId.Value;
            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(emporiumId);

            if (guildSettings.EconomyType == "Disabled") return;

            var economy = _factory.Create(guildSettings.EconomyType);

            switch (notification.ProductListing.Status)
            {
                case ListingStatus.Withdrawn:
                case ListingStatus.Expired:
                    if (!item.Offers.Any()) return;

                    if (notification.ProductListing is VickreyAuction auction)
                    {
                        foreach (var bid in item.Offers)
                        {
                            var user = EmporiumUser.Create(new EmporiumId(emporiumId), bid.UserId, bid.UserReference);

                            await economy.IncreaseBalanceAsync(user, bid.Amount, $"Bid returned for {item.Quantity} {item.Title}");
                            await Task.Delay(200, cancellationToken);
                        }
                    }
                    else
                    {
                        var bid = item.Offers.OrderBy(x => x.SubmittedOn).Last();
                        var user = EmporiumUser.Create(new EmporiumId(emporiumId), bid.UserId, bid.UserReference);

                        await economy.IncreaseBalanceAsync(user, bid.Amount, $"Bid returned for {item.Quantity} {item.Title}");
                    }
                    break;
                case ListingStatus.Sold:
                    if (notification.ProductListing is not VickreyAuction listing) return;
                    if (item.Offers.Count == 1) return;

                    var orderedOffers = item.Offers.OrderByDescending(x => x.Amount.Value).ToArray();
                    var refund = Money.Create(orderedOffers[0].Amount.Value - orderedOffers[1].Amount.Value, item.StartingPrice.Currency);

                    foreach (var bid in orderedOffers.Skip(1))
                    {
                        var user = EmporiumUser.Create(new EmporiumId(emporiumId), bid.UserId, bid.UserReference);

                        await economy.IncreaseBalanceAsync(user, bid.Amount, $"Bid returned for {item.Quantity} {item.Title}");
                        await Task.Delay(200, cancellationToken);
                    }

                    var winningBid = orderedOffers[0];
                    var winner = EmporiumUser.Create(new EmporiumId(emporiumId), winningBid.UserId, winningBid.UserReference);

                    await economy.IncreaseBalanceAsync(winner, refund, $"Partial bid refund for {item.Quantity} {item.Title}");

                    break;
                default:
                    break;
            }

            return;
        }
    }

    internal class ScheduledListingRemoved : INotificationHandler<ListingRemovedNotification>
    {
        private readonly IMediator _meidator;
        private readonly ICurrentUserService _userService;
        private readonly IGuildSettingsService _settingsService;

        public ScheduledListingRemoved(IMediator mediator, ICurrentUserService userService, IGuildSettingsService settingsService)
        {
            _meidator = mediator;
            _userService = userService;
            _settingsService = settingsService;
        }

        public async Task Handle(ListingRemovedNotification notification, CancellationToken cancellationToken)
        {
            if (!notification.ProductListing.IsScheduled) return;
            if (notification.ProductListing.Status == ListingStatus.Withdrawn) return;

            _userService.CurrentUser = notification.ProductListing.Owner;

            await _meidator.Send(new RepostListingCommand(notification.EmporiumId, notification.ShowroomId, notification.ProductListing), cancellationToken);
            
            var settingsCache = (GuildSettingsCacheService)_settingsService;
            await settingsCache.UpdateGuildSettingsAync(await settingsCache.GetGuildSettingsAsync(notification.EmporiumId.Value));
        }
    }

    internal class ListingRemovedQueue : INotificationHandler<ListingRemovedNotification> 
    {
        private readonly IProductQueueService<Command<Listing>, Listing> _queueService;

        public ListingRemovedQueue(IProductQueueService<Command<Listing>, Listing> productQueueService)
        {
            _queueService = productQueueService;
        }

        public async Task Handle(ListingRemovedNotification notification, CancellationToken cancellationToken)
        {
            await _queueService.DequeueAsync(notification.ProductListing.Id);
        }
    }
}
