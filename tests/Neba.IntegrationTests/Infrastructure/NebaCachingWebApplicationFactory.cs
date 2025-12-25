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
/// WebApplicationFactory specifically for caching integration tests.
/// Unlike NebaWebApplicationFactory, this DOES apply the caching decorator to test caching functionality.
/// </summary>
public sealed class NebaCachingWebApplicationFactory(WebsiteDatabase database)
        : WebApplicationFactory<IApiAssemblyMarker>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:website", database.ConnectionString);
        builder.UseSetting("ConnectionStrings:hangfire", database.ConnectionString);

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
            // Remove all Hangfire infrastructure to prevent initialization issues
#pragma warning disable CA2263 // Prefer generic overload - can't use generic version for Hangfire types we don't reference
            services.RemoveAll(typeof(Hangfire.JobStorage));
            services.RemoveAll(typeof(Hangfire.IGlobalConfiguration));
#pragma warning restore CA2263
            services.RemoveAll<IHostedService>();

            // Replace real background job scheduler with no-op implementation
            services.RemoveAll<Neba.Application.BackgroundJobs.IBackgroundJobScheduler>();
            services.AddScoped<Neba.Application.BackgroundJobs.IBackgroundJobScheduler, NoOpBackgroundJobScheduler>();

            // Ensure each test gets a fresh distributed cache instance to avoid cache pollution between tests
            services.RemoveAll<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            services.AddDistributedMemoryCache();

            // Ensure each test gets a fresh HybridCache instance
            services.RemoveAll<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
            services.AddHybridCache();

            // Register test query handlers for caching tests
            services.Scan(scan => scan
                .FromAssemblyOf<NebaCachingWebApplicationFactory>()
                .AddClasses(classes => classes.AssignableTo(typeof(Neba.Application.Messaging.IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Apply caching decorator to ALL handlers (including test handlers) for caching tests
            services.Decorate(typeof(Neba.Application.Messaging.IQueryHandler<,>), typeof(Neba.Infrastructure.Caching.CachedQueryHandlerDecorator<,>));
        });
    }
}
