using Microsoft.Extensions.Logging;

namespace Neba.Website.Application.Documents.Bylaws;

internal static partial class GetBylawsQueryHandlerLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Bylaws document not found in storage, retrieving from source")]
    public static partial void LogRetrievingFromSource(this ILogger<GetBylawsQueryHandler> logger);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Triggered background job to sync bylaws document to storage")]
    public static partial void LogTriggeredBackgroundSync(this ILogger<GetBylawsQueryHandler> logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to trigger background sync for bylaws document, but continuing with response")]
    public static partial void LogFailedToTriggerBackgroundSync(
        this ILogger<GetBylawsQueryHandler> logger,
        Exception ex);
}
