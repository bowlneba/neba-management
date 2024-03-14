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
        Debug.Assert(configuration != null, nameof(configuration) + " != null");

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        #region Feature Flags

#if DEBUG

        services.AddScopedFeatureManagement(configuration.GetSection("FeatureManagement"));

#else
        AddFeatureManagement(services, configuration);

#endif

        #endregion

        services.AddDiagnostics();
        services.AddCaching();
        services.AddHealthChecks(configuration);

        return services;
    }

#if !DEBUG
    private static void AddFeatureManagement(IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddAzureAppConfiguration();

        var connectionString = configuration.GetConnectionString("AppConfig") ??
                                  throw new InvalidOperationException("AppConfig ConnectionString is not set");

        configuration.AddAzureAppConfiguration(options 
            => options.Connect(connectionString)
                .UseFeatureFlags(flagOptions =>
                {
                    flagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(10);
                    flagOptions.Select(KeyFilter.Any);
                }));

        services.AddScopedFeatureManagement();
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

    private static void AddCaching(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();

        services.AddSingleton<ICacheService, CacheService>();
    }

    private static void AddHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddSqlServer(config.GetConnectionString("HealthCheck") ??
                          throw new InvalidOperationException("Cannot get HealthCheck ConnectionString"));
    }
}