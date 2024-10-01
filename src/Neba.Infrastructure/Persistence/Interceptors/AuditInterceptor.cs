using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Neba.Application.Clock;

namespace Neba.Infrastructure.Persistence.Interceptors;

internal sealed class AuditInterceptor
    : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<AuditInterceptor> _logger;

    private readonly List<AuditEntry> _auditEntries;

    public AuditInterceptor(List<AuditEntry> auditEntries, IDateTimeProvider dateTimeProvider, ILogger<AuditInterceptor> logger)
    {
        _auditEntries = auditEntries;

        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        _logger.LogSavingChangesBegin();

        if (eventData.Context is null)
        {
            _logger.LogEventDataContextNull();

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var startTime = _dateTimeProvider.UtcNow;
        var transactionId = Guid.CreateVersion7();

        var auditEntries = eventData.Context.ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is not AuditEntry && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => new AuditEntry
            {
                Id = Guid.CreateVersion7(),
                TransactionId = transactionId,
                StartTimeUtc = startTime,
                Metadata = entry.DebugView.LongView
            }).ToList();

        if (auditEntries.Count == 0)
        {
            _logger.LogNoAuditEntriesRequired();

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        _logger.LogAuditEntries(auditEntries);
        _auditEntries.AddRange(auditEntries);

        _logger.LogSavingChangesComplete();

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        _logger.LogSavedChangesBegin();

        if (eventData.Context is null)
        {
            _logger.LogEventDataContextNull();

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var endTime = _dateTimeProvider.UtcNow;

        _logger.LogAuditEntriesMarkedAsSucceeded();

        foreach (var auditEntry in _auditEntries)
        {
            auditEntry.EndTimeUtc = endTime;
            auditEntry.Succeeded = true;
        }

        if (_auditEntries.Count > 0)
        {
            //todo: offload this to a background task
            eventData.Context.Set<AuditEntry>().AddRange(_auditEntries);
            _auditEntries.Clear();

            await eventData.Context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogSavedChangesAsyncComplete();

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogSavedChangesFailedBegin();

        if (eventData.Context is null)
        {
            _logger.LogEventDataContextNull();

            return;
        }

        var endTime = _dateTimeProvider.UtcNow;

        _logger.LogAuditEntriesMarkedAsFailed();

        foreach (var auditEntry in _auditEntries)
        {
            auditEntry.EndTimeUtc = endTime;
            auditEntry.Succeeded = false;
            auditEntry.ErrorMessage = eventData.Exception.Message;
        }

        if (_auditEntries.Count > 0)
        {
            //todo: offload this to a background task
            eventData.Context.Set<AuditEntry>().AddRange(_auditEntries);
            _auditEntries.Clear();

            await eventData.Context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogSavedChangesFailedAsyncComplete();

        await base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}

internal static partial class AuditInterceptorLogMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "AuditInterceptor.SavingChangesAsync beginning")]
    public static partial void LogSavingChangesBegin(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "The DbContext is null in the event data")]
    public static partial void LogEventDataContextNull(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "No audit entries required")]
    public static partial void LogNoAuditEntriesRequired(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Audit entries: {@AuditEntries}")]
    public static partial void LogAuditEntries(this ILogger<AuditInterceptor> logger, List<AuditEntry> auditEntries);

    [LoggerMessage(Level = LogLevel.Trace, Message = "AuditInterceptor.SavingChangesAsync completed")]
    public static partial void LogSavingChangesComplete(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "AuditInterceptor.SavedChangesAsync beginning")]
    public static partial void LogSavedChangesBegin(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Audit entries marked as succeeded")]
    public static partial void LogAuditEntriesMarkedAsSucceeded(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "AuditInterceptor.SavedChangesAsync completed")]
    public static partial void LogSavedChangesAsyncComplete(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "AuditInterceptor.SavedChangesFailedAsync beginning")]
    public static partial void LogSavedChangesFailedBegin(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Audit entries marked as failed")]
    public static partial void LogAuditEntriesMarkedAsFailed(this ILogger<AuditInterceptor> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "AuditInterceptor.SavedChangesFailedAsync completed")]
    public static partial void LogSavedChangesFailedAsyncComplete(this ILogger<AuditInterceptor> logger);
}