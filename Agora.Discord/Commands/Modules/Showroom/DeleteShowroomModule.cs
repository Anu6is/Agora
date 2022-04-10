using Disqord.Bot;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Qmmands;

namespace Agora.Discord.Commands.Modules
{
    [Group("Delete")]
    public sealed class DeleteShowroomModule : AgoraModuleBase
    {
        [Command("Auction_Room")]
        [Description("Delete an existing auction room")]
        public async Task<DiscordCommandResult> DeleteAuctionRoom([Description("If no channel is specified, the current channel is used.")] ShowroomId room = null)
        {
            await ExecuteAsync(new DeleteShowroomCommand(EmporiumId, room ?? ShowroomId, "auctionitem"));

            return Reply("Auction room deleted.");
        }
    }
}
