using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Neba.Endpoints;

/// <summary>
/// Provides extension methods for configuring Neba endpoints.
/// </summary>
public static class EndpointsConfiguration
{
    /// <summary>
    /// Adds Neba endpoints to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the endpoints to.</param>
    /// <returns>The service collection with the added endpoints.</returns>
    public static IServiceCollection AddNebaEndpoints(this IServiceCollection services)
    {
        services.AddFastEndpoints(config =>
            config.Assemblies = [typeof(EndpointsConfiguration).Assembly]);

        return services;
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