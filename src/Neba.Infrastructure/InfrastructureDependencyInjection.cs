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

        Debug.Assert(configuration != null, nameof(configuration) + " != null");

        services.AddDiagnostics();

        return services;
    }

    private static void AddDiagnostics(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();

        services.AddScoped<IObserver<KeyValuePair<string, object?>>, KeyValueObserver>();
        services.AddScoped<IObserver<DiagnosticListener>, DiagnosticObserver>();

        DiagnosticListener.AllListeners.Subscribe(services.BuildServiceProvider()
            .GetRequiredService<IObserver<DiagnosticListener>>());
    }
}