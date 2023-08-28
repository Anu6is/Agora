using Agora.Shared;
using FastEndpoints;

namespace Agora.API.Features.Shard
{
    public record ShardStatus(string State);

    public class Endpoint : EndpointWithoutRequest<ShardStatus>
    {
        public IBotStatisticsService BotService { get; set; }

        public override void Configure()
        {
            AllowAnonymous();
            Get("/api/state/shard/{index}");
        }

        public override async Task HandleAsync(CancellationToken c)
        {
            int index = Route<int>("index");
            await SendAsync(new ShardStatus(BotService.GetShardState(index)));
        }
    }
}