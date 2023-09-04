//using Agora.API.PreProcessors;
//using Agora.API.Services;
using FastEndpoints;

namespace Agora.API.Features.Guilds
{
    public record Membership(Guild[] Guilds);
    public record Guild(ulong Id, string Name, string Icon, bool Owner, ulong Permissions, string[] Features);

    public class Endpoint: EndpointWithoutRequest<string>
    {
        public override void Configure()
        {
            Get("/api/discord/guilds/{userId}");
        }

        public override async Task HandleAsync(CancellationToken c)
        {
            var userId = Route<string>("userId");
            //var tokenService = Resolve<AccessTokenService>();

            //try
            //{
            //    var user = await tokenService.GetUserInfoAsync(userId!);
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}

            //await SendAsync(new Membership());

            await SendAsync(userId!);
        }
    }
}
