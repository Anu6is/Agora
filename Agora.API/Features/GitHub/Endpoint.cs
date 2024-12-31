using FastEndpoints;
using Microsoft.Extensions.Configuration;

namespace Agora.API.Features.GitHub;

public class Endpoint : EndpointWithoutRequest<string>
{
    public IConfiguration Configuration { get; set; }

    public override void Configure()
    {
        AllowAnonymous();
        Get("/api/PersonalAccessToken");
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var pat = Configuration["Token:PersonalAccessToken"] ?? string.Empty;

        await SendAsync(pat, cancellation: c);
    }
}
