using Disqord.Bot;
using Disqord;
using Emporia.Application.Features.Commands;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord.Features.Commands;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agora.Discord.Commands.Modules
{
    [Group("Server")]
    public sealed class SetupModule : AgoraModuleBase
    {
        [Command("Setup")]
        public async Task<DiscordCommandResult> ServerSetup()
        {
            await Data.BeginTransactionAsync(async () =>
            {
                var emporiumId = new EmporiumId(Context.GuildId);
                var currentTime = DateTime.Now.ToString("HH:mm");

                await ExecuteAsync(new CreateEmporiumCommand(emporiumId) { LocalTime = Time.From(currentTime) });
                await ExecuteAsync(new CreateGuildSettingsCommand(Context.GuildId, Currency.Create("$", 0)));
                await ExecuteAsync(new CreateCurrencyCommand(emporiumId, "$", 0));
            });

            return Response("Server setup successful!");
        }

        [Command("Settings")]
        public DiscordCommandResult ServerSettings()
        {
            return Response("Server setup successful!");
        }

        [Command("Reset")]
        public async Task<DiscordCommandResult> ServerReset()
        {
            await ExecuteAsync(new DeleteEmporiumCommand(new EmporiumId(Context.GuildId)));
            return Response("Server reset successful!");
        }
    }
}
