using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Domain.Entities;
using Emporia.Domain.Services;
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
                if (_channels.TryAdd(listingId, Channel.CreateUnbounded<(IProductListingBinder, RequestHandlerDelegate<TResponse>)>(new UnboundedChannelOptions())))
                {
                    _tasks.Add(CreateAsync(_channels[listingId].Reader));
                }
            }

            return;
        }

        private async Task CreateAsync(ChannelReader<(IProductListingBinder Request, RequestHandlerDelegate<TResponse> Response)> reader, CancellationToken cancellationToken = default)
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                while (reader.TryRead(out var item))
                {
                    TResponse result = default;

                    try
                    {
                        var cache = await RefreshProductAsync(item.Request.Showroom);

                        if (cache is not null && cache.Listings.Count != 0 && cache.Listings.First().CurrentOffer is not null)
                        {
                            Offer offer = cache.Listings.First().Product switch
                            {
                                GiveawayItem giveaway => giveaway.Offers.OrderByDescending(x => x.SubmittedOn).First(),
                                AuctionItem auction => auction.Offers.OrderByDescending(x => x.SubmittedOn).First(),
                                MarketItem market => market.Offers.OrderByDescending(x => x.SubmittedOn).First(),
                                TradeItem trade => trade.Offers.OrderByDescending(x => x.SubmittedOn).First(),
                                _ => null
                            };

                            item.Request.Showroom.Listings.ElementAt(0).UpdateCurrentOffer(offer);
                        }

                        result = await item.Response();
                        var requestId = item.Request is ICommand<TResponse> cmd ? cmd.Id : (item.Request as ICommand).Id;

                        _logger.LogTrace("[{commandId}] | Processing {command}", requestId, item.Request.GetType().Name);

                        OnRequestProcessed(new RequestEventArgs(item.Request, result));
                    }
                    catch (Exception ex)
                    {
                        OnRequestProcessed(new RequestEventArgs(item.Request, ex));
                    }
                    finally
                    {
                        var success = result is IResult response && response.IsSuccessful;

                        if (item.Request.Showroom.Listings.Count > 0 && success)
                            await _cache.AddShowroomListingAsync(item.Request.Showroom);

                    }
                }
            };
        }

        public async Task EnqueueAsync(TRequest command, RequestHandlerDelegate<TResponse> next)
        {
            if (command is not IProductListingBinder binder) throw new InvalidOperationException();

            await _channels[binder.Showroom.Listings.First().Id].Writer.WriteAsync((binder, next));
        }

        protected virtual void OnRequestProcessed(RequestEventArgs args) => RequestProcessed?.Invoke(args);

        public async Task<Showroom> RefreshProductAsync(Showroom showroom)
        {
            var cache = await _cache.GetShowroomListingAsync(showroom);

            return cache;
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
