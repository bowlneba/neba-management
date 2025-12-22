using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;
using Neba.Application.Storage;

namespace Neba.Application.Documents;

internal sealed class SyncHtmlDocumentToStorageJobHandler
    : IBackgroundJobHandler<SyncHtmlDocumentToStorageJob>
{
    private readonly IDocumentsService _documentsService;
    private readonly IStorageService _storageService;
    private readonly ILogger<SyncHtmlDocumentToStorageJobHandler> _logger;
    public SyncHtmlDocumentToStorageJobHandler(
        IDocumentsService documentsService,
        IStorageService storageService,
        ILogger<SyncHtmlDocumentToStorageJobHandler> logger)
    {
        _documentsService = documentsService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task ExecuteAsync(SyncHtmlDocumentToStorageJob job, CancellationToken cancellationToken)
    {
        _logger.LogStartingHtmlDocumentSync();

        string documentHtml = await _documentsService.GetDocumentAsHtmlAsync(
            job.DocumentKey,
            cancellationToken);

        Dictionary<string, string> metadata = new()
        {
            { "syncedAt", DateTime.UtcNow.ToString("o") }
        };

        string location = await _storageService.UploadAsync(job.ContainerName, job.DocumentName, documentHtml, MediaTypeNames.Text.Html, metadata, cancellationToken);

        _logger.LogCompletedHtmlDocumentSync(location);
    }
}
