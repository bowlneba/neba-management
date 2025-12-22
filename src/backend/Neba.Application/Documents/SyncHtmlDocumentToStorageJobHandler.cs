using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;
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
public sealed class SyncHtmlDocumentToStorageJobHandler
    : IBackgroundJobHandler<SyncHtmlDocumentToStorageJob>
{
    private readonly IDocumentsService _documentsService;
    private readonly IStorageService _storageService;
    private readonly ILogger<SyncHtmlDocumentToStorageJobHandler> _logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="SyncHtmlDocumentToStorageJobHandler"/> class.
    /// </summary>
    /// <param name="documentsService">Service for retrieving documents as HTML.</param>
    /// <param name="storageService">Service for uploading content to storage containers.</param>
    /// <param name="logger">Logger used to record sync progress and results.</param>
    public SyncHtmlDocumentToStorageJobHandler(
        IDocumentsService documentsService,
        IStorageService storageService,
        ILogger<SyncHtmlDocumentToStorageJobHandler> logger)
    {
        _documentsService = documentsService;
        _storageService = storageService;
        _logger = logger;
    }

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
        _logger.LogStartingHtmlDocumentSync();

        string documentHtml = await _documentsService.GetDocumentAsHtmlAsync(
            job.DocumentKey,
            cancellationToken);

        Dictionary<string, string> metadata = new()
        {
            { "synced_at", DateTime.UtcNow.ToString("o") }
        };

        string location = await _storageService.UploadAsync(job.ContainerName, job.DocumentName, documentHtml, MediaTypeNames.Text.Html, metadata, cancellationToken);

        _logger.LogCompletedHtmlDocumentSync(location);
    }
}
