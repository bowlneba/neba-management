using ErrorOr;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Awards;
using Neba.Application.Bowlers.BowlerAwards;
using Neba.Application.Bowlers.BowlerTitles;

namespace Neba.Application;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S1144

/// <summary>
/// Provides extension methods for registering application services in the dependency injection container.
/// </summary>
public static class ApplicationDependencyInjection
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds application services to the service collection.
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddApplication()
        {
            services
            .AddBowlersUseCases()
            .AddAwardsUseCases();

            return services;
        }

        private IServiceCollection AddBowlersUseCases()
        {
            services.AddScoped<IQueryHandler<BowlerTitlesQuery, ErrorOr<BowlerTitlesDto>>, BowlerTitlesQueryHandler>();
            services.AddScoped<IQueryHandler<ListBowlerTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>, ListBowlerTitlesQueryHandler>();
            services.AddScoped<IQueryHandler<ListBowlerTitleSummariesQuery, IReadOnlyCollection<BowlerTitleSummaryDto>>, ListBowlerTitleSummariesQueryHandler>();

            return services;
        }

        private IServiceCollection AddAwardsUseCases()
        {
            services.AddScoped<IQueryHandler<ListBowlerOfTheYearAwardsQuery, IReadOnlyCollection<BowlerOfTheYearDto>>, ListBowlerOfTheYearAwardsQueryHandler>();

            return services;
        }
    }
}
