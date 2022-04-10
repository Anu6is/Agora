using Disqord.Bot;
using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using Sentry;
using Command = Emporia.Application.Common.Command;

namespace Agora.Discord.Commands
{
    public abstract class AgoraModuleBase : DiscordGuildModuleBase
    {
        private static int _activeCommands;
        
        public IHub SentryHub { get; private set; }
        public IDisposable SentryScope { get; private set; }

        public IDataAccessor Data { get; private set; }
        public IMediator Mediator { get; private set; }
        public IServiceScope MediatorScope { get; private set; }
        public IEmporiaCacheService Cache { get; private set; }
        public IDiscordGuildSettings Settings { get; private set; }

        [DoNotInject]
        public bool RebootInProgress { get; set; } 
        [DoNotInject]
        public bool ShutdownInProgress { get; set; }
        public IGuildSettingsService SettingsService { get; set; }

        public EmporiumId EmporiumId => new(Context.GuildId);
        public ShowroomId ShowroomId => new(Context.ChannelId);


        protected override async ValueTask BeforeExecutedAsync()
        {
            Interlocked.Increment(ref _activeCommands);

            Settings = await SettingsService.GetGuildSettingsAsync(Context.GuildId);
            Cache = Context.Services.GetRequiredService<IEmporiaCacheService>();

            CreateMediatorScope();
            CreateSentryScope();

            await base.BeforeExecutedAsync();

            return;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Command<TResult> command)
        {
            return await Mediator.Send(command);
        }

        public async Task<Unit> ExecuteAsync(Command command)
        {
            return await Mediator.Send(command);
        }

        protected override ValueTask AfterExecutedAsync()
        {
            Interlocked.Decrement(ref _activeCommands);           

            SentryScope.Dispose();
            MediatorScope.Dispose();

            return base.AfterExecutedAsync();
        }

        private void CreateMediatorScope()
        {
            MediatorScope = Context.Services.CreateScope();
            Data = MediatorScope.ServiceProvider.GetRequiredService<IDataAccessor>();
            Mediator = MediatorScope.ServiceProvider.GetRequiredService<IMediator>();
        }

        private void CreateSentryScope()
        {
            using (var scope = Context.Services.CreateScope())
            {
                SentryHub = scope.ServiceProvider.GetRequiredService<IHub>();

                SentryScope = SentryHub.PushScope();

                SentryHub.ConfigureScope(scope =>
                {
                    scope.AddBreadcrumb($"Executing {Context.Command.Name}");

                    scope.SetTag("Command", Context.Command.Name);
                    scope.SetTag("Context", Context.GuildId.ToString());

                    scope.User = new User() { Id = Context.Author.Id.ToString(), Username = Context.Author.Tag };

                    scope.Contexts.TryAdd($"Message: {Context.Message.Id}",
                                          new
                                          {
                                              Guild = Context.Guild.Name,
                                              Channel = Context.Channel.Name,
                                              Command = $"{Context.Command.FullAliases[0]} {Context.RawArguments}"
                                          });
                });
            }
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
                Logger.LogInformation("Task cancellation requested");
            }
        }
    }
}
