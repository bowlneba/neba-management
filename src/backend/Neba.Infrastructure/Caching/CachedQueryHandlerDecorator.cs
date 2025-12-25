using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Neba.Application.Messaging;

namespace Neba.Infrastructure.Caching;

internal sealed class CachedQueryHandlerDecorator<TQuery, TResponse>
    : IQueryHandler<TQuery, TResponse>
      where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _innerHandler;
    private readonly HybridCache _cache;
    private readonly ILogger<CachedQueryHandlerDecorator<TQuery, TResponse>> _logger;
    private readonly bool _isCacheable;

    public CachedQueryHandlerDecorator(
        IQueryHandler<TQuery, TResponse> innerHandler,
        HybridCache cache,
        ILogger<CachedQueryHandlerDecorator<TQuery, TResponse>> logger)
    {
        _innerHandler = innerHandler;
        _cache = cache;
        _logger = logger;
        _isCacheable = typeof(ICachedQuery<TResponse>).IsAssignableFrom(typeof(TQuery));
    }

    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        // Fast path: non-cached queries bypass caching entirely
        if (!_isCacheable || query is not ICachedQuery<TResponse> cachedQuery)
        {
            return await _innerHandler.HandleAsync(query, cancellationToken);
        }

        // Cached path: use HybridCache
        string cacheKey = cachedQuery.Key;

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
                Expiration = cachedQuery.Expiry
            },
            tags: cachedQuery.Tags,
            cancellationToken: cancellationToken);
    }
}
