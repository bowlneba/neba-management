using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Neba.Web.Diagnostics;

internal static class OpenTelemetryConfigurationExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry([NotNull] this WebApplicationBuilder builder)
    {
        const string serviceName = "Neba.Web";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName)
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("neba.web.version",
                            Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    }))
            .WithTracing(tracing =>
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                        options.Endpoint = new Uri(builder.Configuration.GetValue<string>("Jaeger")!))
            );

        return builder;
    }
}