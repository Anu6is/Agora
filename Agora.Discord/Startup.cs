using Agora.Discord.Extensions;
using Agora.Discord.Logging;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Emporia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Emporia.Extensions.Discord;

namespace Agora.Discord
{
    internal static class Startup
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
#if DEBUG
                   .UseEnvironment("Development")
#endif
                   .ConfigureAppConfiguration(builder => builder.AddCommandLine(args))
                   .ConfigureLogging((context, builder) => builder.AddSentry(context).ReplaceDefaultLogger().WithSerilog(context))
                   .ConfigureEmporiaServices()
                   .UseEmporiaDiscordExtension()
                   .ConfigureCustomAgoraServices()
                   .ConfigureDiscordBotSharder((context, bot) =>
                   {
                       bot.UseMentionPrefix = true;
                       bot.Status = Disqord.UserStatus.Invisible;
                       bot.Token = context.Configuration["Discord:Token"];
                       bot.Intents = GatewayIntent.Guilds | GatewayIntent.Integrations 
                                   | GatewayIntent.GuildMessages | GatewayIntent.GuildReactions 
                                   | GatewayIntent.DirectMessages | GatewayIntent.DirectReactions;
                   })
                    .UseDefaultServiceProvider(x =>
                    {
                        x.ValidateScopes = true;
                        x.ValidateOnBuild = true;
                    });
    }
}
