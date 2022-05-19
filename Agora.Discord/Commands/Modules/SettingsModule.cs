using Agora.Addons.Disqord.Checks;
using Agora.Addons.Disqord.Menus;
using Agora.Addons.Disqord.Menus.View;
using Agora.Shared.Cache;
using Agora.Shared.Extensions;
using Disqord;
using Disqord.Bot;
using Emporia.Application.Common;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using Emporia.Extensions.Discord.Features.Commands;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Agora.Discord.Commands
{
    [Group("Server")]
    public sealed class SettingsModule : AgoraModuleBase
    {
        [Command("Setup")]
        [SkipAuthentication]
        [RequireUnregisteredServer]
        public async Task<DiscordCommandResult> ServerSetup(
            [Description("Log all sold/expired items to this channel.")] ITextChannel resultLog,
            [Description("Log all item activity to this channel.")] ITextChannel auditLog = null,
            [Description("Default currency symbol.")] string symbol = "$",
            [Description("Number of decimal places to show for prices.")] int decimalPlaces = 2,
            [Description("Current server time (24-Hour format). Defaults to UTC")] string serverTime = null)
        {
            DefaultDiscordGuildSettings settings = null;
            
            await Data.BeginTransactionAsync(async () =>
            {
                var time = serverTime is null ? Time.From(DateTimeOffset.UtcNow.TimeOfDay) : Time.From(serverTime);
                var emporium = await ExecuteAsync(new CreateEmporiumCommand(EmporiumId) { LocalTime = time });
                var currency = await ExecuteAsync(new CreateCurrencyCommand(EmporiumId, symbol, decimalPlaces));
                
                settings = await ExecuteAsync(new CreateGuildSettingsCommand(Context.GuildId, currency, resultLog.Id)
                {
                    AuditLogChannelId = auditLog?.Id ?? 0ul,
                    TimeOffset = emporium.TimeOffset
                });

                await SettingsService.AddGuildSettingsAsync(settings);
                await Cache.AddEmporiumAsync(emporium);
            });

            var settingsContext = new GuildSettingsContext(Context.Guild, settings, Context.Services.CreateScope().ServiceProvider);
            var options = new List<GuildSettingsOption>() { };

            return View(new ServerSetupView(settingsContext, options));
        }

        [RequireSetup]
        [Command("Settings")]
        public async Task<DiscordCommandResult> ServerSettings()
        {
            await Cache.GetEmporiumAsync(Context.GuildId);
            
            var settingsContext = new GuildSettingsContext(Context.Guild, Settings, Context.Services.CreateScope().ServiceProvider);
            
            return View(new MainSettingsView(settingsContext));
        }

        [RequireSetup]
        [Command("Rooms")]
        public async Task<DiscordCommandResult> ServerRooms()
        {
            var response = await Cache.GetShowroomsAsync(Context.GuildId.RawValue);
            var showrooms = response.Data.Select(details => details.ToShowroomModel()).ToList();            
            var settingsContext = new GuildSettingsContext(Context.Guild, Settings, Context.Services.CreateScope().ServiceProvider);
                
            return View(new MainShowroomView(settingsContext, showrooms));
        }

        [RequireSetup]
        [Command("Reset")]
        public async Task<DiscordCommandResult> ServerReset()
        {
            await ExecuteAsync(new DeleteEmporiumCommand(new EmporiumId(Context.GuildId)));
            
            (Cache as EporiaCacheService).Clear(Context.GuildId);

            return Reply("Server reset successful!");
        }
    }
}
