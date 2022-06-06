using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Agora.Shared
{
    public interface ICommandModuleBase
    {
        ILogger Logger { get; }
        IDataAccessor Data { get; }
        IMediator Mediator { get; }
        ITransaction Transaction { get; }
        IEmporiaCacheService Cache { get; }
        IDiscordGuildSettings Settings { get; }
        IGuildSettingsService SettingsService { get; }

        public EmporiumId EmporiumId { get; }
        public ShowroomId ShowroomId { get; }

        public async Task<TResult> ExecuteAsync<TResult>(Command<TResult> command)
        {
            return await Mediator.Send(command);
        }

        public async Task<Unit> ExecuteAsync(Command command)
        {
            return await Mediator.Send(command);
        }

        public async Task WaitForCommandsAsync(Func<bool> condition, int waitTimeInMinutes, CancellationToken cancellationToken)
            => await WaitWhileAsync(condition, TimeSpan.FromMinutes(waitTimeInMinutes), cancellationToken);

        private async Task WaitWhileAsync(Func<bool> condition, TimeSpan timeout, CancellationToken cancellationToken, int pollDelay = 100)
        {
            if (cancellationToken.IsCancellationRequested) return;

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task waitTask = WaitWhileAsync(condition, cts.Token, pollDelay);
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            Task finishedTask = await Task.WhenAny(waitTask, timeoutTask);

            if (!cancellationToken.IsCancellationRequested)
            {
                cts.Cancel();
                await finishedTask;
            }
        }

        private async Task WaitWhileAsync(Func<bool> condition, CancellationToken cancellationToken, int pollDelay = 100)
        {
            try
            {
                while (condition())
                    await Task.Delay(pollDelay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                Logger.LogInformation("Task cancellation requested");
            }
        }
    }
}
