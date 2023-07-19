using Agora.Shared.EconomyFactory;
using Emporia.Application.Common;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Domain.Events;
using Emporia.Domain.Services;
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
            if (notification.ProductListing.Product is TradeItem) return;

            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(notification.ProductListing.Owner.EmporiumId.Value);

            if (guildSettings.EconomyType == "Disabled") return;

            var product = notification.ProductListing.Product;
            var economy = _factory.Create(guildSettings.EconomyType);
            var submission = product switch
            {
                AuctionItem auction => auction.Offers.OrderByDescending(x => x.SubmittedOn).First().Amount,
                MarketItem market => market.Offers.OrderByDescending(x => x.SubmittedOn).First().Amount,
                GiveawayItem giveaway => notification.ProductListing is StandardGiveaway ? null : Money.Create(giveaway.Offers.Sum(x => giveaway.TicketPrice.Value), giveaway.TicketPrice.Currency),
                _ => throw new InvalidOperationException($"Cannot increase balances for {product.GetType()}")
            };

            if (submission is null) return;

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

            var emporiumId = notification.ProductListing.Owner.EmporiumId.Value;
            var guildSettings = await _guildSettingsService.GetGuildSettingsAsync(emporiumId);

            if (guildSettings.EconomyType == "Disabled") return;

            var economy = _factory.Create(guildSettings.EconomyType);

            if (notification.ProductListing.Product is AuctionItem auction)
                await AuctionRemovedAsync(notification, auction, economy, cancellationToken);
            else if (notification.ProductListing is RaffleGiveaway raffle && raffle.Product is GiveawayItem giveaway)
                await RaffleRemovedAsync(raffle, giveaway, economy, cancellationToken);
            else if (notification.ProductListing is StandardMarket { AllowOffers: true } market)
                await MarketItemRemovedAsync(notification, (MarketItem)market.Product, economy, cancellationToken);

            return;
        }

        private static async Task MarketItemRemovedAsync(ListingRemovedNotification notification, MarketItem item, IEconomy economy, CancellationToken cancellationToken)
        {
            var emporiumId = notification.ProductListing.Owner.EmporiumId.Value;

            switch (notification.ProductListing.Status)
            {
                case ListingStatus.Withdrawn:
                case ListingStatus.Expired:
                    if (!item.Offers.Any()) return;

                    var payment = item.Offers.OrderBy(x => x.SubmittedOn).Last();
                    var user = EmporiumUser.Create(new EmporiumId(emporiumId), payment.UserId, payment.UserReference);

                    await economy.IncreaseBalanceAsync(user, payment.Amount, $"Offer returned for {item.Quantity} {item.Title}");
                    break;
                case ListingStatus.Sold:
                    if (item.Offers.Count <= 1) return;

                    var offers = item.Offers.OrderByDescending(x => x.SubmittedOn);

                    if (offers.First().Amount != item.CurrentPrice) return;

                        var previousOffer = offers.Skip(1).First();
                    var refundee = EmporiumUser.Create(new EmporiumId(emporiumId), previousOffer.UserId, previousOffer.UserReference);

                    await economy.IncreaseBalanceAsync(refundee, previousOffer.Amount, $"Offer returned for {item.Quantity} {item.Title}");
                    break;
                default:
                    break;
            }
        }

        private static async Task RaffleRemovedAsync(RaffleGiveaway raffle, GiveawayItem item, IEconomy economy, CancellationToken cancellationToken)
        {
            var emporiumId = raffle.Owner.EmporiumId.Value;

            switch (raffle.Status)
            {
                case ListingStatus.Withdrawn:
                case ListingStatus.Expired:
                    if (!item.Offers.Any()) return;

                    foreach (var ticket in item.Offers)
                    {
                        var user = EmporiumUser.Create(new EmporiumId(emporiumId), ticket.UserId, ticket.UserReference);

                        await economy.IncreaseBalanceAsync(user, item.TicketPrice, $"Ticket refunded for {item.Title} raffle");
                        await Task.Delay(200, cancellationToken);
                    }
                    break;
                case ListingStatus.Sold:
                    break;
                default:
                    break;
            }
        }

        private static async Task AuctionRemovedAsync(ListingRemovedNotification notification, AuctionItem item, IEconomy economy, CancellationToken cancellationToken)
        {
            var emporiumId = notification.ProductListing.Owner.EmporiumId.Value;

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
            if (notification.ProductListing.ReschedulingChoice == RescheduleOption.Never) return;
            if (notification.ProductListing.Status == ListingStatus.Withdrawn) return;

            var status = notification.ProductListing.Status;
            var choice = notification.ProductListing.ReschedulingChoice;

            if (status == ListingStatus.Expired && choice == RescheduleOption.Sold) return;
            if (status == ListingStatus.Sold && choice == RescheduleOption.Expired) return;

            _userService.CurrentUser = notification.ProductListing.Owner;

            await _meidator.Send(new RepostListingCommand(notification.EmporiumId, notification.ShowroomId, notification.ProductListing), cancellationToken);

            await _settingsService.UpdateGuildSettingsAync(await _settingsService.GetGuildSettingsAsync(notification.EmporiumId.Value));
        }
    }

    internal class ListingRemovedQueue : INotificationHandler<ListingRemovedNotification>
    {
        private readonly IProductQueueService<Command<IResult<Listing>>, IResult<Listing>> _queueService;

        public ListingRemovedQueue(IProductQueueService<Command<IResult<Listing>>, IResult<Listing>> productQueueService)
        {
            _queueService = productQueueService;
        }

        public async Task Handle(ListingRemovedNotification notification, CancellationToken cancellationToken)
        {
            await _queueService.DequeueAsync(notification.ProductListing.Id);
        }
    }
}
