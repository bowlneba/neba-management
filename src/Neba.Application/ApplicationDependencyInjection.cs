using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Behaviors;

namespace Neba.Application;

/// <summary>
/// Provides extension methods for adding application services to the dependency injection container.
/// </summary>
public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Adds shared application services to the specified <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The web application builder to add the services to.</param>
    /// <returns>The web application builder with the added services.</returns>
    public static WebApplicationBuilder AddSharedApplicationServices([NotNull] this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return builder;
    }
}