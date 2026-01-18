using System.Diagnostics;
using System.Net.Mime;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Neba.Application.BackgroundJobs;
using Neba.Application.Storage;

namespace Neba.Application.Documents;

/// <summary>
/// Background job handler that synchronizes an HTML document from the
/// application's document store to an external storage container.
/// </summary>
/// <remarks>
/// This handler retrieves document HTML via <see cref="IDocumentsService"/>
/// and uploads it using <see cref="IStorageService"/>. Logging is performed
/// at start and completion of the sync operation.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="SyncHtmlDocumentToStorageJobHandler"/> class.
/// </remarks>
/// <param name="documentsService">Service for retrieving documents as HTML.</param>
/// <param name="storageService">Service for uploading content to storage containers.</param>
/// <param name="cache">Distributed cache for temporary storage.</param>
/// <param name="notifier">Notifier for document refresh status updates.</param>
/// <param name="logger">Logger used to record sync progress and results.</param>
public sealed class SyncHtmlDocumentToStorageJobHandler(
    IDocumentsService documentsService,
    IStorageService storageService,
    HybridCache cache,
    IDocumentRefreshNotifier notifier,
    ILogger<SyncHtmlDocumentToStorageJobHandler> logger)
        : IBackgroundJobHandler<SyncHtmlDocumentToStorageJob>
{
    private static readonly ActivitySource s_activitySource = new("Neba.BackgroundJobs");

    /// <summary>
    /// Executes the sync job: retrieves the document HTML and uploads it to the
    /// specified storage container and path.
    /// </summary>
    /// <param name="job">The sync job containing container, path, and document key.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the sync operation finishes.</returns>
    public async Task ExecuteAsync(SyncHtmlDocumentToStorageJob job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (string.IsNullOrWhiteSpace(job.DocumentKey))
        {
            throw new ArgumentException("Job.DocumentKey cannot be null or whitespace.", nameof(job));
        }

        if (string.IsNullOrWhiteSpace(job.Container))
        {
            throw new ArgumentException("Job.Container cannot be null or whitespace.", nameof(job));
        }

        if (string.IsNullOrWhiteSpace(job.Path))
        {
            throw new ArgumentException("Job.Path cannot be null or whitespace.", nameof(job));
        }

        using Activity? activity = s_activitySource.StartActivity("backgroundjob.sync_document");

        if (activity is not null)
        {
            activity.SetTag("job.type", "sync_document");
            activity.SetTag("document.key", job.DocumentKey);
            activity.SetTag("storage.container", job.Container);
            activity.SetTag("storage.path", job.Path);
            activity.SetTag("triggered.by", job.TriggeredBy);
        }

        long jobStartTimestamp = Stopwatch.GetTimestamp();
        SyncHtmlDocumentToStorageMetrics.RecordJobStart(job.DocumentKey, job.TriggeredBy);

        try
        {
            logger.LogStartingHtmlDocumentSync();

            await UpdateStatusAsync(job, DocumentRefreshStatus.Retrieving, cancellationToken: cancellationToken);

            // Retrieve document HTML
            long retrieveStartTimestamp = Stopwatch.GetTimestamp();
            string documentHtml = await documentsService.GetDocumentAsHtmlAsync(
                job.DocumentKey,
                cancellationToken);
            double retrieveDurationMs = Stopwatch.GetElapsedTime(retrieveStartTimestamp).TotalMilliseconds;
            SyncHtmlDocumentToStorageMetrics.RecordRetrieveDuration(job.DocumentKey, retrieveDurationMs);

            activity?.AddEvent(new ActivityEvent("document_retrieved", tags: new ActivityTagsCollection
            {
                { "phase", "retrieve" },
                { "duration_ms", retrieveDurationMs }
            }));

            await UpdateStatusAsync(job, DocumentRefreshStatus.Uploading, cancellationToken: cancellationToken);

            job.Metadata["LastUpdatedUtc"] = DateTimeOffset.UtcNow.ToString("o");
            job.Metadata["LastUpdatedBy"] = job.TriggeredBy;

            // Upload document to storage
            long uploadStartTimestamp = Stopwatch.GetTimestamp();
            string name = await storageService.UploadAsync(job.Container, job.Path, documentHtml, MediaTypeNames.Text.Html, job.Metadata, cancellationToken);
            double uploadDurationMs = Stopwatch.GetElapsedTime(uploadStartTimestamp).TotalMilliseconds;
            SyncHtmlDocumentToStorageMetrics.RecordUploadDuration(job.DocumentKey, uploadDurationMs);

            activity?.AddEvent(new ActivityEvent("document_uploaded", tags: new ActivityTagsCollection
            {
                { "phase", "upload" },
                { "duration_ms", uploadDurationMs },
                { "storage.name", name }
            }));

            logger.LogCompletedHtmlDocumentSync(name);

            await UpdateStatusAsync(job, DocumentRefreshStatus.Completed, cancellationToken: cancellationToken);

            // Invalidate caches
            if (!string.IsNullOrWhiteSpace(job.DocumentCacheKey))
            {
                await cache.RemoveAsync(job.DocumentCacheKey, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(job.CacheKey))
            {
                await cache.RemoveAsync(job.CacheKey, cancellationToken);
            }

            double totalDurationMs = Stopwatch.GetElapsedTime(jobStartTimestamp).TotalMilliseconds;
            SyncHtmlDocumentToStorageMetrics.RecordJobSuccess(job.DocumentKey, totalDurationMs);

            activity?.SetTag("job.duration_ms", totalDurationMs);
            activity?.SetTag("retrieve.duration_ms", retrieveDurationMs);
            activity?.SetTag("upload.duration_ms", uploadDurationMs);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            double totalDurationMs = Stopwatch.GetElapsedTime(jobStartTimestamp).TotalMilliseconds;
            SyncHtmlDocumentToStorageMetrics.RecordJobFailure(job.DocumentKey, totalDurationMs, ex.GetType().Name);

            activity?.SetTag("job.duration_ms", totalDurationMs);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            logger.LogErrorDuringHtmlDocumentSync(ex);

            await UpdateStatusAsync(job, DocumentRefreshStatus.Failed, ex.Message, cancellationToken);

            if (!string.IsNullOrWhiteSpace(job.CacheKey))
            {
                await cache.RemoveAsync(job.CacheKey, cancellationToken);
            }

            throw;
        }
    }

    private async Task UpdateStatusAsync(
        SyncHtmlDocumentToStorageJob job,
        DocumentRefreshStatus status,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(job.CacheKey))
        {
            DocumentRefreshJobState? existingState = await cache.GetOrCreateAsync(
                job.CacheKey,
                _ => ValueTask.FromResult<DocumentRefreshJobState?>(null),
                cancellationToken: CancellationToken.None);

            var state = new DocumentRefreshJobState
            {
                DocumentType = job.DocumentKey,
                StartedAt = existingState?.StartedAt ?? DateTimeOffset.UtcNow,
                TriggeredBy = job.TriggeredBy,
                Status = status.Name,
                ErrorMessage = message
            };

            await cache.SetAsync(job.CacheKey, state, new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10)
            }, cancellationToken: CancellationToken.None);
        }

        await notifier.NotifyStatusAsync(job.HubGroupName, status, message, cancellationToken);
    }
}
