using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Emporia.Application.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Agora.Discord.Commands
{
    public sealed class BotControlModule : AgoraModuleBase
    {
        public IHost ApplicationHost { get; set; }

        [Command("test")]
        public DiscordCommandResult Test() => Reply("Success!");

        [Command("Shutdown")]
        public async Task Shutdown()
        {
            Logger.LogInformation("Shutdown requested");

            ShutdownInProgress = true;
            
            await Context.Bot.SetPresenceAsync(UserStatus.DoNotDisturb);

            await WaitForCommandsAsync(1);
            await ApplicationHost.StopAsync(Context.Bot.StoppingToken);
        }
    }
}
