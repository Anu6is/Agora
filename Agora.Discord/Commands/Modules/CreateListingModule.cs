using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agora.Discord.Commands
{
    [Group("Create")]
    public sealed class CreateListingModule
    {
        [Command("Standard_Auction")]
        [Description("Create a standard auction.")]
        public async Task StandardAuction(
            [Description("The title of the listing.")] string title,
            [Description("The description of the listing.")] string description,
            [Description("The price of the listing.")] decimal price,
            [Description("The category of the listing.")] string category,
            [Description("The tags of the listing.")] string[] tags,
            [Description("The image of the listing.")] string image)
        {
            await Task.CompletedTask;
        }

        [Command("Live_Auction")]
        [Description("Create a live auction.")]
        public async Task LiveAuction()
        {
            await Task.CompletedTask;
        }

        [Command("Sealed_Auction")]
        [Description("Create a sealed auction.")]
        public async Task SealedAuction()
        {
            await Task.CompletedTask;
        }

        //market

        //trade

        //exchange
    }
}
