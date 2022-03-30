using Agora.Addons.Disqord.Checks;
using Agora.Addons.Disqord.Menus;
using Agora.Addons.Disqord.Menus.View;
using Disqord.Bot;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Commands;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Agora.Discord.Commands.Modules
{
    [Group("Server")]
    public sealed class SettingsModule : AgoraModuleBase
    {
        public IGuildSettingsService SettingsService { get; set; }

        [Command("Setup")]
        public async Task<DiscordCommandResult> ServerSetup()
        {
            var RESULTS = 0ul; var CURRENCY = "$"; var DECIMALS = 0; var CURRENT_TIME = DateTime.Now.ToString("HH:mm"); var AUDIT = 0ul;

            await Data.BeginTransactionAsync(async () =>
            {
                var emporiumId = new EmporiumId(Context.GuildId);
                var emporium = await ExecuteAsync(new CreateEmporiumCommand(emporiumId)
                {
                    LocalTime = Time.From(CURRENT_TIME)
                });
                var currency = await ExecuteAsync(new CreateCurrencyCommand(emporiumId, CURRENCY, DECIMALS));
                var settings = await ExecuteAsync(new CreateGuildSettingsCommand(Context.GuildId, currency, RESULTS)
                {
                    AuditLogChannelId = AUDIT,
                    TimeOffset = emporium.TimeOffset
                });

                await SettingsService.AddGuildSettingsAsync(settings);
            });

            return Reply("Server setup successful!");
        }
        
        [RequireSetup]
        [Command("Settings")]
        public async Task<DiscordCommandResult> ServerSettings()
            => View(new MainSettingsView(new GuildSettingsContext(await SettingsService.GetGuildSettingsAsync(Context.GuildId), Context.Services.CreateScope().ServiceProvider)));

        [RequireSetup]
        [Command("Reset")]
        public async Task<DiscordCommandResult> ServerReset()
        {
            await ExecuteAsync(new DeleteEmporiumCommand(new EmporiumId(Context.GuildId)));
            return Reply("Server reset successful!");
        }
    }
}
