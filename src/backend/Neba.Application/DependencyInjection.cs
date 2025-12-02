using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitles;

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
            services.AddScoped<IQueryHandler<BowlerTitlesQuery, BowlerTitlesDto?>, BowlerTitlesQueryHandler>();
            services.AddScoped<IQueryHandler<ListBowlerTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>, ListBowlerTitlesQueryHandler>();
            services.AddScoped<IQueryHandler<ListBowlerTitleSummariesQuery, IReadOnlyCollection<BowlerTitleSummaryDto>>, ListBowlerTitleSummariesQueryHandler>();

            return services;
        }
    }
}
