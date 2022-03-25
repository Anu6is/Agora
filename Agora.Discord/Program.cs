using Agora.Discord;
using Emporia.Persistence.Extensions;
using Microsoft.Extensions.Hosting;

try
{
    using var host = Startup.CreateHostBuilder(args).Build();
    await host.MigrateDatabaseAsync();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
