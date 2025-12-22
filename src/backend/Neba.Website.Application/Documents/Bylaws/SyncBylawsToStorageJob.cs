using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;
using Neba.Application.Storage;

namespace Neba.Website.Application.Documents.Bylaws;

internal sealed class SyncBylawsToStorageJob
    : IBackgroundJob
{
    private readonly IDocumentsService _documentsService;
    private readonly IStorageService _storageService;
    private readonly ILogger<SyncBylawsToStorageJob> _logger;

    public SyncBylawsToStorageJob(
        IDocumentsService documentsService,
        IStorageService storageService,
        ILogger<SyncBylawsToStorageJob> logger)
    {
        _documentsService = documentsService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogStartingBylawsSync();

        string bylawsHtml = await _documentsService.GetDocumentAsHtmlAsync(
            GetBylawsQueryHandler.BylawsDocumentName,
            cancellationToken);

        Dictionary<string, string> metadata = new()
        {
            { "syncedAt", DateTime.UtcNow.ToString("o") }
        };

        string location = await _storageService.UploadAsync("documents","bylaws.html",bylawsHtml, MediaTypeNames.Text.Html, metadata, cancellationToken);

        _logger.LogCompletedBylawsSync(location);
    }
}
