using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neba.ServiceDefaults.HealthChecks;

namespace Neba.ServiceDefaults;

#pragma warning disable S1144 // Extensions should be static classes
#pragma warning disable S2325 // Static members should be in static classes
#pragma warning disable CA1034 // Nested types should not be visible

/// <summary>
/// Extension methods for adding service defaults to a host application builder.
/// </summary>
public static class ServiceDefaultsExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {

        /// <summary>
        /// Adds service defaults to the host application builder.
        /// </summary>
        public TBuilder AddServiceDefaults()
        {
            builder
                .AddOpenTelemetry()
                .AddDefaultHealthChecks();

            builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

            return builder;
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Uses service default endpoints in the web application.
        /// </summary>
        public WebApplication UseDefaultEndpoints()
        {
            app.UseDefaultHealthChecks();

            return app;
        }
    }
}
