using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Database;

/// <summary>
/// EF Core interceptor that detects and logs slow database queries.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SlowQueryInterceptor"/> class.
/// </remarks>
/// <param name="logger">Logger for recording slow query warnings.</param>
/// <param name="slowQueryThresholdMs">Threshold in milliseconds for slow query detection.</param>
internal sealed class SlowQueryInterceptor(
    ILogger<SlowQueryInterceptor> logger,
    double slowQueryThresholdMs = 1000) : DbCommandInterceptor
{
    private static readonly ActivitySource ActivitySource = new("Neba.Database");
    private static readonly Meter Meter = new("Neba.Database");

    private static readonly Histogram<double> QueryDuration = Meter.CreateHistogram<double>(
        "neba.database.query.duration",
        unit: "ms",
        description: "Database query execution duration");

    private static readonly Counter<long> SlowQueries = Meter.CreateCounter<long>(
        "neba.database.query.slow",
        description: "Number of slow database queries");

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        RecordQueryMetrics(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        RecordQueryMetrics(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        RecordQueryMetrics(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        RecordQueryMetrics(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        RecordQueryMetrics(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        RecordQueryMetrics(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void RecordQueryMetrics(DbCommand command, CommandExecutedEventData eventData)
    {
        double durationMs = eventData.Duration.TotalMilliseconds;
        string commandType = command.CommandType.ToString();
        bool isSlow = durationMs >= slowQueryThresholdMs;

        // Record metrics
        QueryDuration.Record(
            durationMs,
            new KeyValuePair<string, object?>("db.operation", commandType),
            new KeyValuePair<string, object?>("db.slow", isSlow));

        if (isSlow)
        {
            SlowQueries.Add(1,
                new KeyValuePair<string, object?>("db.operation", commandType),
                new KeyValuePair<string, object?>("db.command_type", commandType));

            // Create span for slow query
            using Activity? activity = ActivitySource.StartActivity(
                "database.slow_query",
                ActivityKind.Internal);

            activity?.SetTag("db.slow_query", true);
            activity?.SetTag("db.statement", command.CommandText);
            activity?.SetTag("db.command_type", commandType);
            activity?.SetTag("db.duration_ms", durationMs);

            logger.LogSlowQueryDetected(command.CommandText ?? "", durationMs, slowQueryThresholdMs);
        }
        else
        {
            logger.LogQueryExecuted(commandType, durationMs);
        }
    }
}
