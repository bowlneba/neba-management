using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neba.Application.Clock;
using Neba.Infrastructure.Clock;
using Neba.Infrastructure.Diagnostics;

#if DEBUG
using Microsoft.FeatureManagement;
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

        configuration.AddFeatureManagement();

#endif

        services.AddDiagnostics();

        return services;
    }

#if !DEBUG

    private static void AddFeatureManagement(this IConfigurationManager configuration)
    {
        configuration.AddAzureAppConfiguration(options =>
        {
            var connectionString = configuration.GetConnectionString("AppConfig") ??
                                  throw new InvalidOperationException("AppConfig ConnectionString is not set");

            options.Connect(connectionString)
                   .UseFeatureFlags();
        });
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
}