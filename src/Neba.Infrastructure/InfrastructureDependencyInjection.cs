using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Clock;
using Neba.Infrastructure.Clock;

namespace Neba.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
