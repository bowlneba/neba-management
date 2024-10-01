using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Persistence.Interceptors;

internal sealed class SlowQueryInterceptor
    : DbCommandInterceptor
{
    private readonly int _slowQueryThresholdInMilliseconds;
    private readonly ILogger<SlowQueryInterceptor> _logger;

    public SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger, int slowQueryThresholdInMilliseconds)
    {
        _slowQueryThresholdInMilliseconds = slowQueryThresholdInMilliseconds;
        _logger = logger;
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        CheckDuration(eventData, command);

        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        CheckDuration(eventData, command);

        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void CheckDuration(CommandEndEventData eventData, DbCommand command)
    {
        _logger.CheckDuration();

        if (eventData.Duration.TotalMilliseconds > _slowQueryThresholdInMilliseconds)
        {
            _logger.LogSlowQuery(command.CommandText, eventData.Duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogQuery(command.CommandText, eventData.Duration.TotalMilliseconds);
        }
    }
}

internal static partial class SlowQueryInterceptorLogMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Checking Query Execution Time")]
    public static partial void CheckDuration(this ILogger<SlowQueryInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow query detected: {Query} took {ElapsedMilliseconds}ms")]
    public static partial void LogSlowQuery(this ILogger<SlowQueryInterceptor> logger, string query, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Query executed: {Query} took {ElapsedMilliseconds}ms")]
    public static partial void LogQuery(this ILogger<SlowQueryInterceptor> logger, string query, double elapsedMilliseconds);
}