using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Services;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

internal sealed class GoogleDocsService(DocumentMapper documentMapper, GoogleDocsSettings settings)
    : IDocumentsService
{
    private readonly GoogleDocsCredentials _credentials = settings.Credentials;
    private readonly DocumentMapper _documentMapper = documentMapper;

    public async Task<string> GetDocumentAsHtmlAsync(string documentId)
    {
        GoogleCredential googleCredential = null!;

        string credentialJson = System.Text.Json.JsonSerializer.Serialize(_credentials);

        ServiceAccountCredential serviceAccountCredential = CredentialFactory.FromJson<ServiceAccountCredential>(credentialJson);
        googleCredential = serviceAccountCredential.ToGoogleCredential()
            .CreateScoped(DocsService.Scope.DocumentsReadonly);

        using var service = new DocsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = googleCredential,
            ApplicationName = "Neba Api"
        });

        DocumentsResource.GetRequest request = service.Documents.Get(documentId);
        Document document = await request.ExecuteAsync();

        return _documentMapper.ConvertToHtml(document);
    }
}
