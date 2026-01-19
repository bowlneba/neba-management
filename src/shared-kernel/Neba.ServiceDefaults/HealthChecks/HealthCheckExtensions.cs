using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Neba.ServiceDefaults.HealthChecks;

#pragma warning disable S1144 // Extensions should be static classes
#pragma warning disable S2325 // Static members should be in static classes
#pragma warning disable CA1034 // Nested types should not be visible

/// <summary>
/// Extension methods for adding health checks to a host application builder.
/// </summary>
public static class HealthCheckExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {

        /// <summary>
        /// Adds service defaults to the host application builder.
        /// </summary>
        internal TBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }
    }

    extension(WebApplication app)
    {
        internal WebApplication UseDefaultHealthChecks()
        {
            app.MapHealthChecks("/health", new()
            {
                Predicate = _ => true,
                ResponseWriter = HealthCheckResponseWriter.Default()
            });

            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });

            return app;
        }
    }
}
