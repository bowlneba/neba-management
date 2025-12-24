using Microsoft.Extensions.Logging;
using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Application.Storage;

namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Handles queries to retrieve organization bylaws documentation as HTML.
/// </summary>
/// <remarks>
/// Uses a cache-aside pattern: attempts to retrieve from storage first (fast path),
/// then falls back to source and triggers background sync (slow path).
/// </remarks>
/// <param name="storageService">Service for retrieving documents from storage.</param>
/// <param name="documentsService">Service for retrieving documents from source.</param>
/// <param name="bylawsSyncJob">Background job for syncing documents to storage.</param>
/// <param name="logger">Logger for diagnostic information.</param>
internal sealed class GetBylawsQueryHandler(
    IStorageService storageService,
    IDocumentsService documentsService,
    IBylawsSyncBackgroundJob bylawsSyncJob,
    ILogger<GetBylawsQueryHandler> logger)
        : IQueryHandler<GetBylawsQuery, DocumentDto>
{
    private readonly IStorageService _storageService = storageService;
    private readonly IDocumentsService _documentsService = documentsService;
    private readonly IBylawsSyncBackgroundJob _bylawsSyncJob = bylawsSyncJob;
    private readonly ILogger<GetBylawsQueryHandler> _logger = logger;

    /// <summary>
    /// Retrieves the organization bylaws document as HTML.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organization bylaws as an HTML string.</returns>
    public async Task<DocumentDto> HandleAsync(
        GetBylawsQuery request,
        CancellationToken cancellationToken)
    {
        // Fast path: try to get from storage cache
        if (await _storageService.ExistsAsync(BylawsConstants.ContainerName, BylawsConstants.FileName, cancellationToken))
        {
            return await _storageService.GetContentWithMetadataAsync(
                BylawsConstants.ContainerName,
                BylawsConstants.FileName,
                cancellationToken);
        }

        // Slow path: get from source and trigger background sync
        _logger.LogRetrievingFromSource();

        string documentHtml = await _documentsService.GetDocumentAsHtmlAsync(
            BylawsConstants.DocumentKey,
            cancellationToken);

        // Trigger background job to cache the document (fire-and-forget)
        try
        {
            _bylawsSyncJob.TriggerImmediateSync();
            _logger.LogTriggeredBackgroundSync();
        }
#pragma warning disable CA1031 // Catching all exceptions is intentional to ensure resilience - background job failure should not break the request
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogFailedToTriggerBackgroundSync(ex);
        }

        return new DocumentDto
        {
            Content = documentHtml,
            Metadata = new Dictionary<string, string>
            {
                ["LastUpdatedUtc"] = DateTimeOffset.UtcNow.ToString("o"),
                ["LastUpdatedBy"] = "System"
            }
        };
    }
}
