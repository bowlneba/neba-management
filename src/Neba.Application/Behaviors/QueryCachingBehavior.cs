using System.Diagnostics;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Neba.Application.Caching;

namespace Neba.Application.Behaviors;

public sealed class QueryCachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, ErrorOr<TResponse>>
    where TRequest : ICacheQuery

{
    private readonly ICacheService _cacheService;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

    public QueryCachingBehavior(IFeatureManager featureManager, ICacheService cacheService,
        ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _featureManager = featureManager;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ErrorOr<TResponse>> Handle(TRequest request, RequestHandlerDelegate<ErrorOr<TResponse>> next,
        CancellationToken cancellationToken)
    {
        Debug.Assert(next != null, nameof(next) + " != null");

        if (!await _featureManager.IsEnabledAsync(FeatureFlags.Caching))
        {
            return await next();
        }

        var cachedResponse = await _cacheService.GetAsync<TResponse>(request.Key, cancellationToken);

        var requestName = typeof(TRequest).Name;

        if (cachedResponse is not null)
        {
            _logger.CacheHit(requestName);

            return cachedResponse;
        }

        _logger.CacheMiss(requestName);

        var response = await next();

        if (!response.IsError)
        {
            await _cacheService.SetAsync(request.Key, response.Value, request.Expiration, cancellationToken);
        }

        return response;
    }
}

internal static partial class QueryCachingBehaviorLogMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Cache hit for {RequestName}")]
    public static partial void CacheHit(this ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Cache miss for {RequestName}")]
    public static partial void CacheMiss(this ILogger logger, string requestName);
}