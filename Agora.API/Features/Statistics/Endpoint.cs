using Agora.Shared;
using Emporia.Extensions.Discord.Features.Queries;
using FastEndpoints;
using Humanizer;
using MediatR;

namespace Agora.API.Features.Statistics
{
    public record Statistics(string Servers, string Showrooms, string Users, string Listings);
    
    public class Endpoint : EndpointWithoutRequest<Statistics>
    {
        public IBotStatisticsService BotService { get; set; }
        public IMediator Mediator { get; set; }

        public override void Configure()
        {
            AllowAnonymous();
            Get("/api/stats");
            ResponseCache(3600);
        }

        public override async Task HandleAsync(CancellationToken c)
        {
            var listings = await Mediator.Send(new GetTotalListingsQuery(), c);
            var showrooms = await Mediator.Send(new GetTotalShowroomsQuery(), c);

            await SendAsync(new Statistics(BotService.GetTotalGuilds().ToMetric(decimals:1),
                                         showrooms.Data.ToMetric(decimals: 1),
                                         BotService.GetTotalMembers().ToMetric(decimals: 1),
                                         listings.Data.ToMetric(decimals: 1)));
        }
    }
}
