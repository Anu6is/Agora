using Agora.Addons.Disqord.Checks;
using Disqord;
using Disqord.Bot;
using Emporia.Application.Features.Commands;
using Emporia.Application.Models;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Commands;
using Qmmands;

namespace Agora.Discord.Commands
{
    [RequireSetup]
    [Group("Create")]
    [RequireBotChannelPermissions(Permission.SendMessages | Permission.SendEmbeds | Permission.ManageThreads)]
    public class CreateMarketModule : AgoraModuleBase
    {
        [Group("Standard")]
        public class StandardMarketModule : AgoraModuleBase 
        {
            [Command("Market")]
            public async Task CreateStandarMarket(
                [Description("Length of time the item is available.")] TimeSpan duration,
                [Description("Title of the item to be sold.")] ProductTitle title,
                [Description("Price at which the item is being sold.")] decimal price,
                [Description("Currency to use. Defaults to server default")] string currency = null,
                [Description("When the item would be available. Defaults to now.")] DateTime? scheduledStart = null,
                [Description("Quantity available. Defaults to 1.")] Stock quantity = null,
                [Description("Url of image to include. Can also be attached.")] string imageUrl = null,
                [Description("Category the item is associated with")] CategoryTitle category = null,
                [Description("Subcategory to list the item under. Requires category.")] SubcategoryTitle subcategory = null,
                [Description("Additional information about the item.")] ProductDescription description = null,
                [Description("A hidden message to be sent to the buyer.")] HiddenMessage message = null,
                [Description("The type of discount to aplly.")] Discount discountType = Discount.None,
                [Description("The amount of discount to apply.")] decimal discountAmount = 0,
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
                    Discount = discountType,
                    DiscountValue = discountAmount,
                    HiddenMessage = message,
                    Anonymous = anonymous
                };

                await ExecuteAsync(new CreateStandardMarketCommand(showroom, item, listing));

                _ = ExecuteAsync(new UpdateGuildSettingsCommand((DefaultDiscordGuildSettings)Settings));
            }
        }
    }
}
//[Description("Price for one item. Quantity must be more than 1")] decimal pricePerUnit = 0,
