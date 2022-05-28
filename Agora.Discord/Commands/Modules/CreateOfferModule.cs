using Agora.Addons.Disqord.Checks;
using Disqord;
using Disqord.Bot;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Qmmands;

namespace Agora.Discord.Commands
{
    [RequireSetup]
    public class CreateOfferModule : AgoraModuleBase
    {
        [Command("Bid")]
        [RequireBarterChannel]
        public async Task<DiscordCommandResult> AddBid([Description("The amount to bid on the item listing.")] decimal amount)
        {
            var room = (Context.Channel as IThreadChannel);
            
            await ExecuteAsync(new CreateBidCommand(EmporiumId, new ShowroomId(room.ChannelId.RawValue) , ReferenceNumber.Create(room.Id.RawValue), amount));

            return Reply("Bid Submitted!");
        }

        //[Command("Pay")]
        //[RequireBarterChannel]
        //public async Task<DiscordCommandResult> SubmitPayment()
        //{
        //    var room = (Context.Channel as IThreadChannel);

        //    await ExecuteAsync(new SubmitPaymentCommand(EmporiumId, new ShowroomId(room.ChannelId.RawValue), ReferenceNumber.Create(room.Id.RawValue)));

        //    return Reply("Payment Submitted!");
        //}
    }
}