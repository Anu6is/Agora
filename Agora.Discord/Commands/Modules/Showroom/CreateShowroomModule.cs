using Disqord.Bot;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Qmmands;

/*
 Auction 
    |_ Room
        |_ Add
        |_ Remove
        |_ Hours

 Create
    |_ Standard
        |_ Auction
        |_ Market
        |_ Trade
        |_ Exhange
    |_ Live
        |_ Auction
    |_ Sealed
        |_ Auction
    |_ Discount
        |_ Market
    |_ Task
        |_ Market
    |_ Open
        |_ Trade
        |_ Exchange

 Extend
    |_ Auction
        |_ By
        |_ To
    |_ Market
        |_ By
        |_ To
    |_ Trade
        |_ By
        |_ To
    |_ Exchange
        |_ By
        |_ To
*/

namespace Agora.Discord.Commands.Modules
{
    [Group("Add")]
    public sealed class CreateShowroomModule : AgoraModuleBase
    {
        [Command("Auction_Room")]
        [Description("Create a new auction room")]
        public async Task<DiscordCommandResult> CreateAuctionRoom(
            [Description("If no channel is specified, the current channel is used.")] ShowroomId room = null,
            [Description("Time the room starts accepting listings/offers (24-hour format)")] string opensAt = null,
            [Description("Time the room stops accepting listings/offers (24-hour format)")] string closesAt = null)
        {
            await ExecuteAsync(new CreateShowroomCommand<AuctionItem>(EmporiumId, room ?? ShowroomId)
            {
                OpensAt = opensAt,
                ClosesAt = closesAt
            });
            
            return Reply("Showroom registered!");
        }
        // hours 

        // close

        // open
    }
}
