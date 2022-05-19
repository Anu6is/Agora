using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;

namespace Agora.Discord.Services
{
    public class DiscordSyncService : DiscordBotService
    {
        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            await Client.SetPresenceAsync(UserStatus.Online);
            
            return;
        }
    }
}
