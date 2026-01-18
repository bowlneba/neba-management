using System.Diagnostics;
using System.Text.Json;
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
    private readonly bool _isErrorOrResponse;
    private readonly Type? _innerType;

    public CachedQueryHandlerDecorator(
        IQueryHandler<TQuery, TResponse> innerHandler,
        HybridCache cache,
        ILogger<CachedQueryHandlerDecorator<TQuery, TResponse>> logger)
    {
        _innerHandler = innerHandler;
        _cache = cache;
        _logger = logger;
        _isCacheable = typeof(ICachedQuery<TResponse>).IsAssignableFrom(typeof(TQuery));
        _isErrorOrResponse = ErrorOrCacheHelper.IsErrorOrType(typeof(TResponse));
        _innerType = _isErrorOrResponse
            ? ErrorOrCacheHelper.GetInnerType(typeof(TResponse))
            : null;
    }

    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        // Fast path: non-cached queries bypass caching entirely
        if (!_isCacheable || query is not ICachedQuery<TResponse> cachedQuery)
        {
            return await _innerHandler.HandleAsync(query, cancellationToken);
        }

        string cacheKey = cachedQuery.Key;

        // Branch: ErrorOr<T> vs plain T
        if (_isErrorOrResponse)
        {
            return await HandleErrorOrCachedQueryAsync(cachedQuery, cacheKey, cancellationToken);
        }

        return await HandlePlainCachedQueryAsync(cachedQuery, cacheKey, cancellationToken);
    }

    private async Task<TResponse> HandleErrorOrCachedQueryAsync(
        ICachedQuery<TResponse> cachedQuery,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        string queryType = typeof(TQuery).Name;

        // Track whether factory was called and capture error result if needed
        bool factoryCalled = false;
        TResponse? errorResult = default;

        // Try to get cached value (caching inner type T, not ErrorOr<T>)
        object? cachedValue = await _cache.GetOrCreateAsync<object?>(
            cacheKey,
            async cancel =>
            {
                factoryCalled = true;
                _logger.LogCacheMiss(cacheKey);
                CacheMetrics.RecordCacheMiss(cacheKey, queryType);

                TResponse response = await _innerHandler.HandleAsync((TQuery)cachedQuery, cancel);

                // Don't cache errors - return null to signal bypass
                if (ErrorOrCacheHelper.IsError(response!))
                {
                    _logger.LogErrorResultNotCached(cacheKey);
                    errorResult = response; // Save error to avoid double execution
                    return null;
                }

                // Unwrap and cache inner value
                _logger.LogUnwrappingErrorOr(cacheKey);
                return ErrorOrCacheHelper.GetValue(response!)!;
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = cachedQuery.Expiry
            },
            tags: cachedQuery.Tags,
            cancellationToken: cancellationToken);

        double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

        // Handle null result (error or cache miss)
        if (cachedValue is null)
        {
            CacheMetrics.RecordOperationDuration(durationMs, cacheKey, queryType, hit: false);

            if (factoryCalled)
            {
                // Factory executed and returned error - return saved result (avoid double execution)
                return errorResult!;
            }
            else
            {
                // Cache hit with null from previous error (real HybridCache) - re-execute
                return await _innerHandler.HandleAsync((TQuery)cachedQuery, cancellationToken);
            }
        }

        // Cache hit - record metrics
        if (!factoryCalled)
        {
            _logger.LogCacheHit(cacheKey);
            CacheMetrics.RecordCacheHit(cacheKey, queryType);
        }

        CacheMetrics.RecordOperationDuration(durationMs, cacheKey, queryType, hit: !factoryCalled);

        // HybridCache deserializes as JsonElement when using object type
        // Convert back to the actual inner type
        object deserializedValue = cachedValue is JsonElement jsonElement
            ? JsonSerializer.Deserialize(jsonElement.GetRawText(), _innerType!)!
            : cachedValue;

        // Rewrap cached value back into ErrorOr<T>
        _logger.LogRewrappingErrorOr(cacheKey);
        object wrapped = ErrorOrCacheHelper.WrapValue(_innerType!, deserializedValue);
        return (TResponse)wrapped;
    }

    private async Task<TResponse> HandlePlainCachedQueryAsync(
        ICachedQuery<TResponse> cachedQuery,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        string queryType = typeof(TQuery).Name;
        bool factoryCalled = false;

        TResponse result = await _cache.GetOrCreateAsync(
            cacheKey,
            async cancel =>
            {
                factoryCalled = true;
                _logger.LogCacheMiss(cacheKey);
                CacheMetrics.RecordCacheMiss(cacheKey, queryType);

                return await _innerHandler.HandleAsync((TQuery)cachedQuery, cancel);
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = cachedQuery.Expiry
            },
            tags: cachedQuery.Tags,
            cancellationToken: cancellationToken);

        double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

        if (!factoryCalled)
        {
            _logger.LogCacheHit(cacheKey);
            CacheMetrics.RecordCacheHit(cacheKey, queryType);
        }

        CacheMetrics.RecordOperationDuration(durationMs, cacheKey, queryType, hit: !factoryCalled);

        return result;
    }
}
