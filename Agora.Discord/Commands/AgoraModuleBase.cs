using Disqord.Bot;
using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using Command = Emporia.Application.Common.Command;

namespace Agora.Discord.Commands
{
    public abstract class AgoraModuleBase : DiscordGuildModuleBase
    {
        private static int _activeCommands;

        [DoNotInject]
        public bool RebootInProgress { get; set; }
        [DoNotInject]
        public bool ShutdownInProgress { get; set; }

        public IDataAccessor Data { get; private set; }
        public IMediator Mediator { get; private set; }
        public IEmporiaCacheService Cache { get; private set; }
        public IDiscordGuildSettings Settings { get; private set; }
        public IGuildSettingsService SettingsService { get; set; }

        public EmporiumId EmporiumId => new(Context.GuildId);
        public ShowroomId ShowroomId => new(Context.ChannelId);

        protected override async ValueTask BeforeExecutedAsync()
        {
            Interlocked.Increment(ref _activeCommands);

            Settings = await SettingsService.GetGuildSettingsAsync(Context.GuildId);
            Cache = Context.Services.GetRequiredService<IEmporiaCacheService>();
            Data = Context.Services.GetRequiredService<IDataAccessor>();
            Mediator = Context.Services.GetRequiredService<IMediator>();

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
                Logger.LogInformation("Task cancellation requested");
            }
        }
    }
}
