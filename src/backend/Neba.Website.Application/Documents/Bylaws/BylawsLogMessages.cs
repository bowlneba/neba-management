using Microsoft.Extensions.Logging;

namespace Neba.Website.Application.Documents.Bylaws;

internal static partial class BylawsLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Starting synchronization of bylaws document to storage.")]
    public static partial void LogStartingBylawsSync(this ILogger<SyncBylawsToStorageJob> logger);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Completed synchronization of bylaws document to storage: {DocumentLocation}")]
    public static partial void LogCompletedBylawsSync(
        this ILogger<SyncBylawsToStorageJob> logger,
        string documentLocation);
}
