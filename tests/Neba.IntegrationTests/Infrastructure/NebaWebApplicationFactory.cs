using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Neba.Api;
using Neba.Tests.Website;

namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing that configures the test database.
/// Each test class should create its own instance of WebsiteDatabase and pass it to this factory.
/// </summary>
public sealed class NebaWebApplicationFactory
    : WebApplicationFactory<IApiAssemblyMarker>
{
    private readonly WebsiteDatabase _database;

    public NebaWebApplicationFactory(WebsiteDatabase database)
    {
        _database = database;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:website", _database.ConnectionString);

        // Mock GoogleDocs credentials for integration tests
        builder.UseSetting("GoogleDocs:Credentials:PrivateKey", "mock-private-key-for-testing");
        builder.UseSetting("GoogleDocs:Credentials:ClientEmail", "mock-test@test-project.iam.gserviceaccount.com");
        builder.UseSetting("GoogleDocs:Credentials:PrivateKeyId", "mock-key-id-12345");
        builder.UseSetting("GoogleDocs:Credentials:ClientX509CertUrl", "https://www.googleapis.com/robot/v1/metadata/x509/mock-test%40test-project.iam.gserviceaccount.com");
    }
}
