using Agora.API.Services;
using Agora.Shared;
using FastEndpoints;

namespace Agora.API.Features.Guilds
{
    public record GuildList(Guild[] Guilds);
    public record Guild(ulong Id, string Name, string Icon, bool Owner, ulong Permissions, string[] Features);

    public class Endpoint : EndpointWithoutRequest<IEnumerable<Guild>>
    {
        public IDiscordBotService BotService { get; set; }
        public DiscordApiService ApiService { get; set; }

        public override void Configure()
        {
            Get("/api/discord/guilds/{userId}");
        }

        public override async Task HandleAsync(CancellationToken c)
        {
            var userId = Route<string>("userId");
            var userGuilds = await ApiService.GetGuildsAsync(userId!);
            var mutualGuilds = BotService.GetMutualGuilds(userGuilds.Select(x => x.Id));

            await SendAsync(userGuilds.Where(guild => mutualGuilds.Contains(guild.Id)), cancellation: c);
        }
    }
}
