using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Emporia.Application.Common;
using Emporia.Extensions.Discord;
using Emporia.Persistence.DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace Agora.Discord.Services
{
    public class DiscordSyncService : DiscordBotService
    {
        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            using (var scope = Client.Services.CreateScope())
            {//TODO - what do we do on startup? Get expiring items
                var dataAccessor = scope.ServiceProvider.GetRequiredService<IDataAccessor>();
                var settings = await dataAccessor.Transaction<GenericRepository<DefaultDiscordGuildSettings>>().ListAsync();
            }
            await Client.SetPresenceAsync(UserStatus.Online);
            
            return;
        }
    }
}
