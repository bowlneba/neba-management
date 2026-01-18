using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Neba.ServiceDefaults;

#pragma warning disable S1144 // Extensions should be static classes
#pragma warning disable S2325 // Static members should be in static classes
#pragma warning disable CA1034 // Nested types should not be visible

internal static class OpenTelemetryExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        public TBuilder AddOpenTelemetry()
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;

                logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: builder.Environment.ApplicationName));
            });

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService(serviceName: builder.Environment.ApplicationName))
                .WithMetrics(metrics => metrics
                    .AddMeter("Neba.*")                // Custom application meters
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation())
                .WithTracing(tracing => tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddSource("Neba.*")              // Custom application sources
                    .AddSource("Azure.Storage.Blobs") // Azure SDK traces
                    .AddAspNetCoreInstrumentation(x => x
                        .Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase) &&
                            !context.Request.Path.StartsWithSegments("/alive", StringComparison.OrdinalIgnoreCase) &&
                            !context.Request.Path.StartsWithSegments("/background-jobs", StringComparison.OrdinalIgnoreCase))
                    .AddHttpClientInstrumentation());

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private TBuilder AddOpenTelemetryExporters()
        {
            bool useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string>("Otel:OtlpEndpoint"));

            if (useOtlpExporter)
            {
                var otelUri = new Uri(builder.Configuration.GetValue<string>("Otel:OtlpEndpoint")!);

                builder.Services.AddOpenTelemetry()
                    .UseOtlpExporter(OpenTelemetry.Exporter.OtlpExportProtocol.Grpc, otelUri);
            }

            if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            {
                builder.Services.AddOpenTelemetry().UseAzureMonitor();
            }

            return builder;
        }
    }
}
