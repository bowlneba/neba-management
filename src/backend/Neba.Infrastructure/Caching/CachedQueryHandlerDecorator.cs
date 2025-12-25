using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Neba.Application.Messaging;

namespace Neba.Infrastructure.Caching;

internal sealed class CachedQueryHandlerDecorator<TQuery, TResponse>
    : IQueryHandler<TQuery, TResponse>
      where TQuery : ICachedQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _innerHandler;
    private readonly HybridCache _cache;
    private readonly ILogger<CachedQueryHandlerDecorator<TQuery, TResponse>> _logger;

    public CachedQueryHandlerDecorator(
        IQueryHandler<TQuery, TResponse> innerHandler,
        HybridCache cache,
        ILogger<CachedQueryHandlerDecorator<TQuery, TResponse>> logger)
    {
        _innerHandler = innerHandler;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        string cacheKey = query.Key;

        return await _cache.GetOrCreateAsync(
            cacheKey,
            (handler: _innerHandler, query, logger: _logger, cacheKey),
            static async (state, cancel) =>
            {
                state.logger.LogCacheMiss(state.cacheKey);

                TResponse response = await state.handler.HandleAsync(state.query, cancel);

                return response;
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = query.Expiry
            },
            tags: query.Tags,
            cancellationToken: cancellationToken);
    }
}
