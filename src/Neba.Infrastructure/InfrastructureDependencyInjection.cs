using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Neba.Application.Caching;
using Neba.Application.Clock;
using Neba.Infrastructure.Caching;
using Neba.Infrastructure.Clock;
using Neba.Infrastructure.Diagnostics;

#if DEBUG
#else
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
#endif

namespace Neba.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

#if DEBUG

        Debug.Assert(configuration != null, nameof(configuration) + " != null");
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));

#else
        AddFeatureManagement(services, configuration);

#endif

        services.AddDiagnostics();

        var cacheConnectionString = configuration.GetConnectionString("Caching") ??
                                    throw new InvalidOperationException("Cache ConnectionString is not set");

        services.AddCaching(cacheConnectionString);

        return services;
    }

#if !DEBUG
    private static void AddFeatureManagement(IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddAzureAppConfiguration();

        configuration.AddAzureAppConfiguration(options =>
        {
            var connectionString = configuration.GetConnectionString("AppConfig") ??
                                  throw new InvalidOperationException("AppConfig ConnectionString is not set");

            options.Connect(connectionString)
                   .UseFeatureFlags(options =>
                   {
                       options.CacheExpirationInterval = TimeSpan.FromSeconds(30);
                       options.Select(KeyFilter.Any);
                   });
        });

        services.AddFeatureManagement();
    }

#endif

    private static void AddDiagnostics(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();

        services.AddScoped<IObserver<KeyValuePair<string, object?>>, KeyValueObserver>();
        services.AddScoped<IObserver<DiagnosticListener>, DiagnosticObserver>();

        DiagnosticListener.AllListeners.Subscribe(services.BuildServiceProvider()
            .GetRequiredService<IObserver<DiagnosticListener>>());
    }

    private static void AddCaching(this IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

        services.AddSingleton<ICacheService, CacheService>();
    }
}