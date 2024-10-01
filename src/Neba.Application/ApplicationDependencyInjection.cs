using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Behaviors;

namespace Neba.Application;

/// <summary>
/// Provides extension methods for adding application services to the dependency injection container.
/// </summary>
public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Adds shared application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddSharedApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}