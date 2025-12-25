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
public sealed class NebaWebApplicationFactory(WebsiteDatabase database)
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
            // (allows background job classes to be constructed without actually scheduling jobs)
            services.RemoveAll<Neba.Application.BackgroundJobs.IBackgroundJobScheduler>();
            services.AddScoped<Neba.Application.BackgroundJobs.IBackgroundJobScheduler, NoOpBackgroundJobScheduler>();

            // Replace HybridCache with no-op implementation to avoid serialization issues
            // The CachedQueryHandlerDecorator will work but won't actually cache anything
            services.RemoveAll<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
            services.AddSingleton<Microsoft.Extensions.Caching.Hybrid.HybridCache, NoOpHybridCache>();

            // Register test query handlers for caching tests
            services.Scan(scan => scan
                .FromAssemblyOf<NebaWebApplicationFactory>()
                .AddClasses(classes => classes.AssignableTo(typeof(Neba.Application.Messaging.IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        });
    }
}

/// <summary>
/// No-op implementation of HybridCache for testing that bypasses caching entirely.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - instantiated by DI
internal sealed class NoOpHybridCache : Microsoft.Extensions.Caching.Hybrid.HybridCache
#pragma warning restore CA1812
{
    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        // Always call the factory - never cache
        return factory(state, cancellationToken);
    }

    public override ValueTask SetAsync<T>(
        string key,
        T value,
        Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        // No-op
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // No-op
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // No-op
        return ValueTask.CompletedTask;
    }
}
