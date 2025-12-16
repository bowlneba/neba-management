using Neba.Infrastructure.Documents;

namespace Neba.Tests;

internal static class GoogleDocsSettingsFactory
{
    public static GoogleDocsSettings Create(params (string Name, string DocumentId, string Route)[] documents)
        => new()
        {
            Credentials = new GoogleDocsCredentials
            {
                Type = "service_account",
                ProjectId = "unit-tests",
                AuthUri = "https://accounts.google.com/o/oauth2/auth",
                TokenUri = "https://oauth2.googleapis.com/token",
                AuthProviderX509CertUrl = "https://www.googleapis.com/oauth2/v1/certs",
                UniverseDomain = "googleapis.com",
                PrivateKey = "-----BEGIN PRIVATE KEY-----\nTEST\n-----END PRIVATE KEY-----\n",
                PrivateKeyId = "test-private-key-id",
                ClientEmail = "unit-tests@example.invalid",
                ClientId = "unit-tests-client-id",
                ClientX509CertUrl = "https://example.invalid/cert"
            },
            Documents = documents
                .Select(d => new GoogleDocument
                {
                    Name = d.Name,
                    DocumentId = d.DocumentId,
                    Route = d.Route
                })
                .ToList()
                .AsReadOnly()
        };
}
