using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Persistence.Interceptors;

internal sealed class SlowQueryInterceptor
    : DbCommandInterceptor
{
    private const int _slowQueryThresholdInMilliseconds = 1500;
    private readonly ILogger<SlowQueryInterceptor> _logger;

    public SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger)
    {
        _logger = logger;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result, CancellationToken cancellationToken = default)
    {
        CheckDuration(eventData, command);

        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        CheckDuration(eventData, command);

        return base.ReaderExecuted(command, eventData, result);
    }

    private void CheckDuration(CommandEndEventData eventData, IDbCommand command)
    {
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
    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow query detected: {Query} took {Duration}ms")]
    public static partial void LogSlowQuery(this ILogger<SlowQueryInterceptor> logger, string query, double duration);

    [LoggerMessage(Level = LogLevel.Information, Message = "Query executed: {Query} took {Duration}ms")]
    public static partial void LogQuery(this ILogger<SlowQueryInterceptor> logger, string query, double duration);
}