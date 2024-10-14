using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Neba.Infrastructure.Diagnostics;

internal static class OpenTelemetryConfigurationExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry([NotNull] this WebApplicationBuilder builder)
    {
        const string serviceName = "Neba.Api";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName)
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("neba.api.version",
                            Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    }))
            .WithTracing(tracing =>
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options =>
                        options.Endpoint = new Uri(builder.Configuration.GetValue<string>("Jaeger")!))
            )
            .WithMetrics(metrics
                => metrics.AddMeter(ApplicationDiagnostics.Meter.Name)
                    .AddPrometheusExporter());

        return builder;
    }

    public static IApplicationBuilder UseOpenTelemetry(this IApplicationBuilder app)
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        return app;
    }
}