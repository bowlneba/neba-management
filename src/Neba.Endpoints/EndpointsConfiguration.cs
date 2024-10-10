using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;

namespace Neba.Endpoints;

/// <summary>
/// Provides extension methods for configuring Neba endpoints.
/// </summary>
public static class EndpointsConfiguration
{
    /// <summary>
    /// Adds Neba endpoints to the specified <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The web application builder to add the endpoints to.</param>
    /// <returns>The web application builder with the added endpoints.</returns>
    public static WebApplicationBuilder AddNebaEndpoints([NotNull] this WebApplicationBuilder builder)
    {
        builder.Services.AddFastEndpoints(config =>
            config.Assemblies = [typeof(EndpointsConfiguration).Assembly]);

        return builder;
    }

    /// <summary>
    /// Configures the application to use Neba endpoints.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder with the configured endpoints.</returns>
    public static IApplicationBuilder UseNebaEndpoints(this IApplicationBuilder app)
    {
        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";

            config.Versioning.Prefix = "v";
            config.Versioning.PrependToRoute = true;
        });

        return app;
    }
}