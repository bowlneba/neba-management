using ErrorOr;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Website.Application.Awards.BowlerOfTheYear;
using Neba.Website.Application.Awards.HighAverage;
using Neba.Website.Application.Awards.HighBlock;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.Documents.Bylaws;
using Neba.Website.Application.Tournaments;
using Neba.Website.Application.Tournaments.GetTournamentRules;

namespace Neba.Website.Application;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable CA1708 // Identifiers should differ by more than case

/// <summary>
/// Extension container that exposes registration helpers for the Website application layer.
/// </summary>
/// <remarks>
/// Keep registrations for application-specific services (use cases, handlers, DTO mappers) here
/// so the composition root can add the entire website application surface with a single call.
/// </remarks>
public static class WebsiteApplicationDependencyInjection
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the Website application services into the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance to allow call chaining from the composition root.
        /// </returns>
        /// <remarks>
        /// This method delegates to more specific registration helpers (e.g. <see cref="AddBowlersUseCases"/>).
        /// </remarks>
        public IServiceCollection AddWebsiteApplication()
        {
            services
                .AddBowlersUseCases()
                .AddAwardsUseCases()
                .AddTournamentsUseCases()
                .AddDocumentsUseCases();

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
            services.AddScoped<IQueryHandler<ListBowlerOfTheYearAwardsQuery, IReadOnlyCollection<BowlerOfTheYearAwardDto>>, ListBowlerOfTheYearAwardsQueryHandler>();
            services.AddScoped<IQueryHandler<ListHigh5GameBlockAwardsQuery, IReadOnlyCollection<HighBlockAwardDto>>, ListHigh5GameBlockAwardsQueryHandler>();
            services.AddScoped<IQueryHandler<ListHighAverageAwardsQuery, IReadOnlyCollection<HighAverageAwardDto>>, ListHighAverageAwardsQueryHandler>();

            return services;
        }

        private IServiceCollection AddTournamentsUseCases()
        {
            services.AddScoped<IQueryHandler<GetTournamentRulesQuery, DocumentDto>, GetTournamentRulesQueryHandler>();
            services.AddScoped<ICommandHandler<RefreshTournamentRulesCacheCommand, string>, RefreshTournamentRulesCacheCommandHandler>();
            services.AddScoped<TournamentRulesSyncBackgroundJob>();

            return services;
        }

        private IServiceCollection AddDocumentsUseCases()
        {
            services.AddScoped<IQueryHandler<GetBylawsQuery, DocumentDto>, GetBylawsQueryHandler>();
            services.AddScoped<ICommandHandler<RefreshBylawsCacheCommand, string>, RefreshBylawsCacheCommandHandler>();
            services.AddScoped<BylawsSyncBackgroundJob>();

            return services;
        }
    }

    extension(IServiceProvider serviceProvider)
    {
        /// <summary>
        /// Initializes background jobs for the Website application (e.g., document syncing).
        /// Call this from the composition root after the application is built.
        /// </summary>
        public void InitializeWebsiteBackgroundJobs()
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            BylawsSyncBackgroundJob bylawsSyncJob = scope.ServiceProvider.GetRequiredService<BylawsSyncBackgroundJob>();
            bylawsSyncJob.RegisterBylawsSyncJob();

            TournamentRulesSyncBackgroundJob tournamentRulesSyncJob = scope.ServiceProvider.GetRequiredService<TournamentRulesSyncBackgroundJob>();
            tournamentRulesSyncJob.RegisterTournamentRulesSyncJob();
        }
    }
}
