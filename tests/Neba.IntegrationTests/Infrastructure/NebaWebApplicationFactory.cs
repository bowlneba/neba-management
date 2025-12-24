using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        builder.UseSetting("ConnectionStrings:hangfire", _database.ConnectionString);

        // Configure Hangfire settings for integration tests
        builder.UseSetting("Hangfire:WorkerCount", "1");
        builder.UseSetting("Hangfire:SucceededJobsRetentionDays", "1");
        builder.UseSetting("Hangfire:DeletedJobsRetentionDays", "1");
        builder.UseSetting("Hangfire:FailedJobsRetentionDays", "1");

        // Mock GoogleDocs credentials for integration tests
        builder.UseSetting("GoogleDocs:Credentials:PrivateKey", "mock-private-key-for-testing");
        builder.UseSetting("GoogleDocs:Credentials:ClientEmail", "mock-test@test-project.iam.gserviceaccount.com");
        builder.UseSetting("GoogleDocs:Credentials:PrivateKeyId", "mock-key-id-12345");
        builder.UseSetting("GoogleDocs:Credentials:ClientX509CertUrl", "https://www.googleapis.com/robot/v1/metadata/x509/mock-test%40test-project.iam.gserviceaccount.com");

        // Suppress verbose logging during tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        // Remove Hangfire entirely from tests - integration tests don't need background jobs
        builder.ConfigureServices(services =>
        {
            // Remove all background job infrastructure to prevent initialization issues
#pragma warning disable CA2263 // Prefer generic overload - can't use generic version for Hangfire types we don't reference
            services.RemoveAll(typeof(Hangfire.JobStorage));
            services.RemoveAll(typeof(Hangfire.IGlobalConfiguration));
#pragma warning restore CA2263
            services.RemoveAll<Neba.Application.BackgroundJobs.IBackgroundJobScheduler>();
            services.RemoveAll<IHostedService>();
        });
    }
}
