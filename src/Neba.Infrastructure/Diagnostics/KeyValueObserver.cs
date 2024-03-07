using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Diagnostics;

internal sealed class KeyValueObserver
    : IObserver<KeyValuePair<string, object?>>
{
    private readonly ILogger<KeyValueObserver> _logger;

    public KeyValueObserver(ILogger<KeyValueObserver> logger)
    {
        _logger = logger;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
        => _logger.ObserverError(error);

    public void OnNext(KeyValuePair<string, object?> value)
    {
        if (value.Key == CoreEventId.ContextInitialized.Name)
        {
            var contextInitializedEventData = value.Value as ContextInitializedEventData;
            _logger.ContextInitialized(contextInitializedEventData?.Context.GetType().Name);

            return;
        }

        if (value.Key != RelationalEventId.ConnectionOpening.Name)
        {
            return;
        }

        var connectionEventData = value.Value as ConnectionEventData;
        _logger.ConnectionOpening(connectionEventData?.Connection.DataSource);
    }
}

internal static partial class KeyValueObserverLogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred in the observer")]
    public static partial void ObserverError(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Context {ContextType} has been initialized")]
    public static partial void ContextInitialized(this ILogger<KeyValueObserver> logger, string? contextType);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Connection to database {DatabaseName} is being opened")]
    public static partial void ConnectionOpening(this ILogger<KeyValueObserver> logger, string? databaseName);
}
