using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Extensions.Discord;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Agora.Shared.Services
{
    public class DefaultProductQueueService<TRequest, TResponse> : IProductQueueService<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public event RequestProcessedHandler RequestProcessed;

        private readonly List<Task> _tasks = new();
        private readonly ConcurrentDictionary<ListingId, Channel<(IProductListingBinder, RequestHandlerDelegate<TResponse>)>> _channels = new();

        private readonly ILogger _logger;
        private readonly IEmporiaCacheService _cache;

        public DefaultProductQueueService(IEmporiaCacheService cacheService, ILogger<DefaultProductQueueService<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _cache = cacheService;
        }

        public void EnsureCreated(ListingId listingId)
        {
            lock (_channels)
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
                            var requestId = item.Request is ICommand<TResponse> cmd ? cmd.Id : (item.Request as ICommand).Id;

                            _logger.LogTrace("[{commandId}] | Processing {command}", requestId, item.Request.GetType().Name);

                            OnRequestProcessed(new RequestEventArgs(item.Request, result));

                            await _cache.AddShowroomListingAsync(item.Request.Showroom);

                            _logger.LogTrace("[{commandId}] | Cache current offer {offer}", requestId, item.Request.Showroom.Listings.FirstOrDefault()?.CurrentOffer?.Submission);

                            await Task.Delay(500);
                        }
                        catch (Exception ex)
                        {
                            OnRequestProcessed(new RequestEventArgs(item.Request, ex));
                        }
                        finally
                        {
                            if (reader.TryPeek(out var nextItem))
                            {
                                var listing = nextItem.Request.Showroom.Listings.First();
                                var showroom = await RefreshProductAsync(nextItem.Request.Showroom);
                                var requestId = item.Request is ICommand<TResponse> cmd ? cmd.Id : (item.Request as ICommand).Id;

                                Offer offer = showroom.Listings.First().Product switch
                                {
                                    AuctionItem auction => auction.Offers.OrderByDescending(x => x.SubmittedOn).First(),
                                    MarketItem market => market.Offers.OrderByDescending(x => x.SubmittedOn).First(),
                                    TradeItem trade => trade.Offers.OrderByDescending(x => x.SubmittedOn).First(),
                                    _ => null
                                };

                                _logger.LogTrace("[{commandId}] | Update current offer {offer} -> {update}", requestId, listing.CurrentOffer?.Submission, offer?.Submission);

                                listing.UpdateCurrentOffer(offer);
                            }
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

        public async Task<Showroom> RefreshProductAsync(Showroom showroom)
        {
            return await _cache.GetShowroomListingAsync(showroom);
        }

        public async Task DequeueAsync(ListingId listingId)
        {
            if (_channels.TryRemove(listingId, out var result))
            {
                await result.Reader.Completion;
                _logger.LogTrace("Dequeue listing [{listingId} ]", listingId);
            }
        }
    }
}
