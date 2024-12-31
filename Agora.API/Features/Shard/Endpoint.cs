using Agora.Shared;
using FastEndpoints;

namespace Agora.API.Features.Shard
{
    public record ShardStatus(int State, string Status);

    public class Endpoint : EndpointWithoutRequest<ShardStatus>
    {
        public IDiscordBotService BotService { get; set; }

        public override void Configure()
        {
            AllowAnonymous();
            Get("/api/state/shard/{index}");
        }

        public override async Task HandleAsync(CancellationToken c)
        {
            var index = Route<int>("index");
            var state = BotService.GetShardState(index);
            var status = state == 0
                ? "Offline"
                : state == 4
                    ? "Online"
                    : "Connecting";

            await SendAsync(new ShardStatus(state, status), cancellation: c);
        }
    }
}