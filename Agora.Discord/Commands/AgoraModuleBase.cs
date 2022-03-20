using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using Sentry;

namespace Agora.Discord.Commands
{
    public abstract class AgoraModuleBase : DiscordModuleBase
    {
        private readonly ILogger _logger;
        private static int _activeCommands;

        public IHub SentryHub { get; private set; }
        public IDisposable SentryScope { get; private set; }

        [DoNotInject]
        public bool RebootInProgress { get; set; } 
        [DoNotInject]
        public bool ShutdownInProgress { get; set; }

        public AgoraModuleBase(ILogger<AgoraModuleBase> logger)
        {
            _logger = logger;
        }

        protected override ValueTask BeforeExecutedAsync()
        {
            Interlocked.Increment(ref _activeCommands);

            

            using (var scope = Context.Services.CreateScope()) 
            {
                SentryHub = scope.ServiceProvider.GetRequiredService<IHub>();                              

                SentryScope = SentryHub.PushScope();
                
                SentryHub.ConfigureScope(scope =>
                {
                    scope.AddBreadcrumb($"Executing {Context.Command.Name}");

                    scope.SetTag("Command", Context.Command.Name);
                    scope.SetTag("Context", Context.GuildId.HasValue ? $"Guild: {Context.GuildId}" : $"Direct: {Context.Author.Id}");

                    scope.User = new User() { Id = Context.Author.Id.ToString(), Username = Context.Author.Tag };

                    var guild = Context.Bot.GetGuild(Context.GuildId.GetValueOrDefault());
                    var channel = Context.Bot.GetChannel(Context.GuildId.GetValueOrDefault(), Context.ChannelId);

                    scope.Contexts.TryAdd($"Message: {Context.Message.Id}",
                                          new
                                          {
                                              Guild = guild?.Name ?? "Direct Message",
                                              Channel = channel?.Name ?? Context.Author.Tag,
                                              Command = $"{Context.Command.FullAliases[0]} {Context.RawArguments}"
                                          });
                });
            }

            return base.BeforeExecutedAsync();  
        }

        protected override ValueTask AfterExecutedAsync()
        {
            Interlocked.Decrement(ref _activeCommands);
            
            SentryHub.AddBreadcrumb($"Exiting {Context.Command.Name}");
            SentryScope.Dispose();
            
            return base.AfterExecutedAsync();
        }

        protected async Task WaitForCommandsAsync(int waitTimeInMinutes) 
            => await WaitWhileAsync(() => _activeCommands > 1, TimeSpan.FromMinutes(waitTimeInMinutes), Context.Bot.StoppingToken);

        private async Task WaitWhileAsync(Func<bool> condition, TimeSpan timeout, CancellationToken ct, int pollDelay = 100)
        {
            if (ct.IsCancellationRequested) return;

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            Task waitTask = WaitWhileAsync(condition, cts.Token, pollDelay);
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            Task finishedTask = await Task.WhenAny(waitTask, timeoutTask);

            if (!ct.IsCancellationRequested)
            {
                cts.Cancel();
                await finishedTask;
            }
        }

        private async Task WaitWhileAsync(Func<bool> condition, CancellationToken ct, int pollDelay = 100)
        {
            try
            {
                while (condition())
                    await Task.Delay(pollDelay, ct);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Task cancellation requested");
            }
        }
    }
}
