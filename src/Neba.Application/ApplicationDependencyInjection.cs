using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Behaviors;

namespace Neba.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddSharedApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
            config.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}