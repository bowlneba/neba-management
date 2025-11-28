using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitleCounts;

namespace Neba.Application;

/// <summary>
/// Provides extension methods for registering application services in the dependency injection container.
/// </summary>
public static class ApplicationDependencyInjection
{
    #pragma warning disable S2325 // Extension methods should be static
    #pragma warning disable CA1034 // Nested types should not be visible
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds application services to the service collection.
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddApplication()
        {
            services.AddBowlersUseCases();

            return services;
        }

        private IServiceCollection AddBowlersUseCases()
        {
            services.AddScoped<IQueryHandler<GetBowlerTitleCountsQuery, IReadOnlyCollection<BowlerTitleCountDto>>, GetBowlerTitleCountsQueryHandler>();
            services.AddScoped<IQueryHandler<GetBowlerTitlesQuery, BowlerTitlesDto?>, GetBowlerTitlesQueryHandler>();
            services.AddScoped<IQueryHandler<GetTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>, GetTitlesQueryHandler>();

            return services;
        }
    }
}
