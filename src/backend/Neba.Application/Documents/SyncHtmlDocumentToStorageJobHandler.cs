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

    /// <summary>
    /// Executes the sync job: retrieves the document HTML and uploads it to the
    /// specified storage container and path.
    /// </summary>
    /// <param name="job">The sync job containing container, document name, and document key.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the sync operation finishes.</returns>
    public async Task ExecuteAsync(SyncHtmlDocumentToStorageJob job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (string.IsNullOrWhiteSpace(job.DocumentKey))
        {
            throw new ArgumentException("Job.DocumentKey cannot be null or whitespace.", nameof(job));
        }

        if (string.IsNullOrWhiteSpace(job.ContainerName))
        {
            throw new ArgumentException("Job.ContainerName cannot be null or whitespace.", nameof(job));
        }

        if (string.IsNullOrWhiteSpace(job.DocumentName))
        {
            throw new ArgumentException("Job.DocumentName cannot be null or whitespace.", nameof(job));
        }

        try
        {
            logger.LogStartingHtmlDocumentSync();

            await UpdateStatusAsync(job, DocumentRefreshStatus.Retrieving, cancellationToken: cancellationToken);

            string documentHtml = await documentsService.GetDocumentAsHtmlAsync(
                job.DocumentKey,
                cancellationToken);

            await UpdateStatusAsync(job, DocumentRefreshStatus.Uploading, cancellationToken: cancellationToken);

            job.Metadata["LastUpdatedUtc"] = DateTimeOffset.UtcNow.ToString("o");
            job.Metadata["LastUpdatedBy"] = job.TriggeredBy;

            string name = await storageService.UploadAsync(job.ContainerName, job.DocumentName, documentHtml, MediaTypeNames.Text.Html, job.Metadata, cancellationToken);

            logger.LogCompletedHtmlDocumentSync(name);

            await UpdateStatusAsync(job, DocumentRefreshStatus.Completed, cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(job.DocumentCacheKey))
            {
                await cache.RemoveAsync(job.DocumentCacheKey, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(job.CacheKey))
            {
                await cache.RemoveAsync(job.CacheKey, cancellationToken);
            }
        }
        catch (Exception ex)
        {
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
