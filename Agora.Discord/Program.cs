using Agora.Addons.Disqord;
using Emporia.Persistence.Extensions;

try
{
    using var host = Startup.CreateWebHost(args); 
    await host.MigrateDatabaseAsync();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}