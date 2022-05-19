using Agora.Addons.Disqord.Checks;
using Disqord;
using Disqord.Bot;
using Emporia.Application.Common;
using Emporia.Application.Features.Commands;
using Emporia.Application.Models;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Commands;
using Qmmands;

namespace Agora.Discord.Commands
{
    [RequireSetup]
    [Group("Create")]
    //send messsage permissions
    //send embed permissions
    //create thread permissions
    public class CreateListingsModule : AgoraModuleBase
    {
        [Group("Standard")]
        public class StandardListingsModule : AgoraModuleBase
        {            
            [Command("Auction")]
            public async Task<DiscordCommandResult> CreateStandarAuction(
                [Description("Length of time the auction should run.")] TimeSpan duration,
                [Description("Title of the item to be auctioned.")] ProductTitle title,
                [Description("Price at which bidding should start at.")] decimal startingPrice,
                [Description("Currency to use. Defaults to server default")] string currency = null,
                [Description("Scheduled start of the auction. Defaults to now.")] DateTime? scheduledStart = null,
                [Description("Quantity available. Defaults to 1.")] Stock quantity = null,
                [Description("Url of image to include. Can also be attached.")] string imageUrl = null,
                [Description("Sell immediately for this price.")] decimal buyNowPrice = 0,
                [Description("Do NOT sell unless bids exceed this price.")] decimal reservePrice = 0,
                [Description("Amount by which bids must increase.")] decimal minBidIncrease = 0,
                [Description("Bid increases cannot exceed this amount.")] decimal maxBidIncrease = 0,
                [Description("Category the item is associated with")] CategoryTitle category = null,
                [Description("Subcategory to list the item under. Requires category.")] SubcategoryTitle subcategory = null,
                [Description("Additional information about the item.")] ProductDescription description = null, 
                [Description("A hidden message to be sent to the winner.")] HiddenMessage message = null,
                [Description("Item owner. Defaults to the command user.")] IMember owner = null,
                [Description("True to hide the item owner.")] bool anonymous = false)
            {
                var emporium = await Cache.GetEmporiumAsync(Context.GuildId);

                quantity ??= Stock.Create(1);
                currency ??= Settings.DefaultCurrency.Symbol;
                scheduledStart ??= emporium.LocalTime.DateTime.AddSeconds(3);
                
                var scheduledEnd = scheduledStart.Value.Add(duration);
                var showroom = new ShowroomModel(EmporiumId, ShowroomId, ListingType.Auction);
                var item = new AuctionItemModel(title, currency, startingPrice, quantity)
                {
                    ImageUrl = imageUrl,
                    Category = category,
                    Subcategory = subcategory,
                    Description = description,
                    ReservePrice = reservePrice, 
                    MinBidIncrease = minBidIncrease, 
                    MaxBidIncrease = maxBidIncrease
                };

                var ownerId = owner?.Id ?? Context.Author.Id;
                var userDetails = await Cache.GetUserAsync(Context.GuildId, ownerId);
                
                var listing = new StandardAuctionModel(scheduledStart.Value, scheduledEnd, new UserId(userDetails.UserId))
                { 
                    BuyNowPrice = buyNowPrice, 
                    HiddenMessage = message, 
                    Anonymous = anonymous 
                };

                await ExecuteAsync(new CreateStandardAuctionCommand(showroom, item, listing));

                _ = ExecuteAsync(new UpdateGuildSettingsCommand((DefaultDiscordGuildSettings)Settings));
                
                return Reply("Auction created.");
            }

            [Command("Market")]
            public async Task<DiscordCommandResult> CreateStandarMarket(
                [Description("Length of time the item is available.")] TimeSpan duration,
                [Description("Title of the item to be sold.")] ProductTitle title,
                [Description("Price at which the item is being sold.")] decimal price,
                [Description("Currency to use. Defaults to server default")] string currency = null,
                [Description("When the item would be available. Defaults to now.")] DateTime? scheduledStart = null,
                [Description("Quantity available. Defaults to 1.")] Stock quantity = null,
                [Description("Price for one item. Quantity must be more than 1")] decimal pricePerUnit = 0,
                [Description("Url of image to include. Can also be attached.")] string imageUrl = null,
                [Description("Category the item is associated with")] CategoryTitle category = null,
                [Description("Subcategory to list the item under. Requires category.")] SubcategoryTitle subcategory = null,
                [Description("Additional information about the item.")] ProductDescription description = null,
                [Description("A hidden message to be sent to the buyer.")] HiddenMessage message = null,
                [Description("Item owner. Defaults to the command user.")] IMember owner = null,
                [Description("True to hide the item owner.")] bool anonymous = false)
            {
                var emporium = await Cache.GetEmporiumAsync(Context.GuildId);

                quantity ??= Stock.Create(1);
                currency ??= Settings.DefaultCurrency.Symbol;
                scheduledStart ??= emporium.LocalTime.DateTime.AddSeconds(3);

                var scheduledEnd = scheduledStart.Value.Add(duration);
                var showroom = new ShowroomModel(EmporiumId, ShowroomId, ListingType.Market);
                var item = new MarketItemModel(title, currency, price, quantity)
                {
                    ImageUrl = imageUrl,
                    Category = category,
                    Subcategory = subcategory,
                    Description = description
                };

                var ownerId = owner?.Id ?? Context.Author.Id;
                var userDetails = await Cache.GetUserAsync(Context.GuildId, ownerId);

                var listing = new StandardMarketModel(scheduledStart.Value, scheduledEnd, new UserId(userDetails.UserId))
                {
                    CostPerItem = pricePerUnit,
                    HiddenMessage = message,
                    Anonymous = anonymous
                };

                await ExecuteAsync(new CreateStandardMarketCommand(showroom, item, listing));

                _ = ExecuteAsync(new UpdateGuildSettingsCommand((DefaultDiscordGuildSettings)Settings));

                return Reply("Market listing created.");
            }
        }
        
    }
}
