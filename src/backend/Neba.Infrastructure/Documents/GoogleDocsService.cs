using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

internal sealed class GoogleDocsService(HtmlProcessor htmlProcessor, GoogleDocsSettings settings)
    : IDocumentsService
{
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
        using var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _lazyCredential.Value,
            ApplicationName = "Neba Api"
        });

        string documentId = _documents.SingleOrDefault(doc => doc.Name == documentName)?.DocumentId
            ?? throw new InvalidOperationException($"Google Document with name '{documentName}' not found in configuration.");

        // Export the document as HTML
        FilesResource.ExportRequest request = service.Files.Export(documentId, "text/html");
        using var stream = new MemoryStream();
        await request.DownloadAsync(stream, cancellationToken);

        // Convert the stream to a string
        string rawHtml = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Post-process the HTML to replace Google Docs links and clean up
        return htmlProcessor.ProcessExportedHtml(rawHtml);
    }
}
