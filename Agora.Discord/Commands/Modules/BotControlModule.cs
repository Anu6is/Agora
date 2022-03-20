using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Agora.Discord.Commands
{
    public sealed class BotControlModule : AgoraModuleBase
    {
        private readonly ILogger _logger;

        public IHost ApplicationHost { get; set; }

        public BotControlModule(ILogger<BotControlModule> logger) : base(logger) => _logger = logger;

        [Command("test")]
        public DiscordCommandResult Test() => Reply("Success!");

        [Command("Shutdown")]
        public async Task Shutdown()
        {
            ShutdownInProgress = true;

            await Context.Bot.SetPresenceAsync(Disqord.UserStatus.DoNotDisturb);

            await WaitForCommandsAsync(1);
            await ApplicationHost.StopAsync(Context.Bot.StoppingToken);
        }
    }
}
