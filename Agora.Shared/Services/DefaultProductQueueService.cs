using Emporia.Application.Common;
using Emporia.Domain.Common;
using MediatR;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Agora.Shared.Services
{
    public class DefaultProductQueueService<TRequest, TResponse> : IProductQueueService<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public event RequestProcessedHandler RequestProcessed;

        private readonly List<Task> _tasks = new();
        private readonly ConcurrentDictionary<ListingId, Channel<(IProductListingBinder, RequestHandlerDelegate<TResponse>)>> _channels = new();

        public void EnsureCreated(ListingId listingId)
        {
            if (_channels.TryAdd(listingId, Channel.CreateBounded<(IProductListingBinder, RequestHandlerDelegate<TResponse>)>(new BoundedChannelOptions(1)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait
            }
            )))
            {
                _tasks.Add(CreateAsync(_channels[listingId].Reader));
            }

            return;
        }

        private Task CreateAsync(ChannelReader<(IProductListingBinder Request, RequestHandlerDelegate<TResponse> Response)> reader, CancellationToken cancellationToken = default)
        {
            var executingTask = Task.Run(async () =>
            {
                while (await reader.WaitToReadAsync(cancellationToken))
                {
                    while (reader.TryRead(out var item))
                    {
                        try
                        {
                            var result = await item.Response();

                            OnRequestProcessed(new RequestEventArgs(item.Request, result));

                            if (reader.TryPeek(out var nextItem))
                                nextItem.Request.Showroom = item.Request.Showroom;

                        }
                        catch (Exception ex)
                        {
                            OnRequestProcessed(new RequestEventArgs(item.Request, ex));
                        }
                    }
                }
            }, cancellationToken);

            return executingTask;
        }

        public async Task EnqueueAsync(TRequest command, RequestHandlerDelegate<TResponse> next)
        {
            if (command is not IProductListingBinder binder) throw new InvalidOperationException();

            await _channels[binder.Showroom.Listings.First().Id].Writer.WriteAsync((binder, next));
        }

        protected virtual void OnRequestProcessed(RequestEventArgs args) => RequestProcessed?.Invoke(args);
    }
}
