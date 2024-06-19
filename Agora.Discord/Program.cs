using Agora.Addons.Disqord;
using Agora.API;
using Emporia.Persistence.Extensions;

try
{
    using var host = Startup.CreateWebHost(args, (_, builder) => builder.ConfigureAgoraAPI())
                            .ConfigureApiApplication();
    await host.MigrateDatabaseAsync();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}