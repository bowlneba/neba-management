using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

internal sealed class GoogleDocsService(
    HtmlProcessor htmlProcessor,
    GoogleDocsSettings settings,
    ILogger<GoogleDocsService> logger)
    : IDocumentsService
{
    private static readonly ActivitySource s_activitySource = new("Neba.GoogleDocs");
    private readonly IReadOnlyCollection<GoogleDocument> _documents = settings.Documents;
    private readonly Lazy<GoogleCredential> _lazyCredential = new(() =>
    {
        string credentialJson = System.Text.Json.JsonSerializer.Serialize(settings.Credentials);
        ServiceAccountCredential serviceAccountCredential = CredentialFactory.FromJson<ServiceAccountCredential>(credentialJson);
        return serviceAccountCredential.ToGoogleCredential()
            .CreateScoped(DriveService.Scope.DriveReadonly);
    });

    public async Task<string> GetDocumentAsHtmlAsync(string documentName, CancellationToken cancellationToken)
    {
        string documentId = _documents.SingleOrDefault(doc => doc.Name == documentName)?.DocumentId
            ?? throw new InvalidOperationException($"Google Document with name '{documentName}' not found in configuration.");

        using Activity? activity = s_activitySource.StartActivity("google.docs.export", ActivityKind.Client);
        activity?.SetTag("document.name", documentName);
        activity?.SetTag("document.id", documentId);
        activity?.SetTag("export.format", "text/html");

        long startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            logger.LogExportingDocument(documentName, documentId);

            using var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _lazyCredential.Value,
                ApplicationName = "Neba Api"
            });

            // Export the document as HTML
            FilesResource.ExportRequest request = service.Files.Export(documentId, "text/html");
            await using var stream = new MemoryStream();
            await request.DownloadAsync(stream, cancellationToken);

            // Convert the stream to a string
            string rawHtml = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            int originalSize = rawHtml.Length;

            logger.LogProcessingHtml(documentName, originalSize);

            // Post-process the HTML to replace Google Docs links and clean up
            string processedHtml = htmlProcessor.ProcessExportedHtml(rawHtml);
            int processedSize = processedHtml.Length;

            logger.LogHtmlProcessed(documentName, processedSize);

            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            logger.LogDocumentExported(documentName, processedSize, durationMs);

            GoogleDocsMetrics.RecordExportSuccess(documentName, documentId, durationMs, processedSize);

            activity?.SetTag("export.size_bytes", processedSize);
            activity?.SetTag("export.duration_ms", durationMs);

            return processedHtml;
        }
        catch (Exception ex)
        {
            logger.LogDocumentExportFailed(ex, documentName, documentId);
            GoogleDocsMetrics.RecordExportFailure(documentName, documentId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
