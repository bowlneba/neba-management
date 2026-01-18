using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Neba.Application.Messaging;

namespace Neba.Infrastructure.Tracing;

/// <summary>
/// Decorator that adds distributed tracing to query handlers via OpenTelemetry.
/// Creates a span for each query execution with relevant metadata.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
internal sealed class TracedQueryHandlerDecorator<TQuery, TResponse>
    : IQueryHandler<TQuery, TResponse>
      where TQuery : IQuery<TResponse>
{
    private static readonly ActivitySource s_activitySource = new("Neba.Handlers");

    private readonly IQueryHandler<TQuery, TResponse> _innerHandler;
    private readonly ILogger<TracedQueryHandlerDecorator<TQuery, TResponse>> _logger;
    private readonly string _queryType;
    private readonly string _responseType;
    private readonly bool _isCached;

    public TracedQueryHandlerDecorator(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<TracedQueryHandlerDecorator<TQuery, TResponse>> logger)
    {
        _innerHandler = innerHandler;
        _logger = logger;
        _queryType = typeof(TQuery).Name;
        _responseType = typeof(TResponse).Name;
        _isCached = typeof(ICachedQuery<TResponse>).IsAssignableFrom(typeof(TQuery));
    }

    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        using Activity? activity = s_activitySource.StartActivity($"query.{_queryType}");

        if (activity is not null)
        {
            activity.SetTag("handler.type", "query");
            activity.SetTag("query.type", _queryType);
            activity.SetTag("response.type", _responseType);
            activity.SetTag("query.cached", _isCached);

            if (_isCached && query is ICachedQuery<TResponse> cachedQuery)
            {
                activity.SetTag("cache.key", cachedQuery.Key);
            }
        }

        long startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            TResponse result = await _innerHandler.HandleAsync(query, cancellationToken);

            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            activity?.SetTag("query.duration_ms", durationMs);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return result;
        }
        catch (Exception ex)
        {
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            activity?.SetTag("query.duration_ms", durationMs);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogQueryExecutionFailed(_queryType, durationMs, ex);

            throw;
        }
    }
}
