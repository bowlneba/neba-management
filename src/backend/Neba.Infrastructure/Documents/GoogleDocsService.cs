using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Services;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

internal sealed class GoogleDocsService(DocumentMapper documentMapper, GoogleDocsSettings settings)
    : IDocumentsService
{
    private readonly IReadOnlyCollection<GoogleDocument> _documents = settings.Documents;
    private readonly Lazy<GoogleCredential> _lazyCredential = new(() =>
    {
        string credentialJson = System.Text.Json.JsonSerializer.Serialize(settings.Credentials);
        ServiceAccountCredential serviceAccountCredential = CredentialFactory.FromJson<ServiceAccountCredential>(credentialJson);
        return serviceAccountCredential.ToGoogleCredential()
            .CreateScoped(DocsService.Scope.DocumentsReadonly);
    });

    public async Task<string> GetDocumentAsHtmlAsync(string documentName, CancellationToken cancellationToken)
    {
        using var service = new DocsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _lazyCredential.Value,
            ApplicationName = "Neba Api"
        });

        string documentId = _documents.SingleOrDefault(doc => doc.Name == documentName)?.DocumentId
            ?? throw new InvalidOperationException($"Google Document with name '{documentName}' not found in configuration.");

        DocumentsResource.GetRequest request = service.Documents.Get(documentId);
        Document document = await request.ExecuteAsync(cancellationToken);

        return documentMapper.ConvertToHtml(document);
    }
}
