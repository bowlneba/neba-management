using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Bowlers;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.BowlingCenters;
using Neba.Website.Application.Tournaments;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.Website.Infrastructure;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S1144 // Remove unused constructor of private type.


/// <summary>
/// Contains dependency injection registration helpers for the Website infrastructure layer.
/// </summary>
/// <remarks>
/// Keep infrastructure-specific registrations (database, repositories, health checks, etc.)
/// consolidated here so the composition root can add the entire website infrastructure surface
/// with a single call to <see cref="AddWebsiteInfrastructure"/>.
/// </remarks>
public static class WebsiteInfrastructureDependencyInjection
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the Website infrastructure services to the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="config">An <c>IConfigurationManager</c> used to read configuration values like connection strings.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when required configuration (for example the "website" connection string) is missing.</exception>
        public IServiceCollection AddWebsiteInfrastructure(IConfigurationManager config)
        {
            ArgumentNullException.ThrowIfNull(config);

            return services
                .AddDatabase(config)
                .AddRepositories();
        }

        private IServiceCollection AddDatabase(IConfigurationManager config)
        {
            string websiteConnectionString = config.GetConnectionString("website")
                                             ?? throw new InvalidOperationException("Database connection string 'website' is not configured.");

            services.AddDbContext<WebsiteDbContext>(options => options
                .UseNpgsql(websiteConnectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, WebsiteDbContext.DefaultSchema)));

            string[] websiteTags = ["database", "website"];

            services.AddHealthChecks()
                .AddDbContextCheck<WebsiteDbContext>(
                    name: "Website Database",
                    tags: websiteTags);

            services.AddRepositories();

            return services;
        }

        private IServiceCollection AddRepositories()
        {
            services.AddScoped<IWebsiteBowlerQueryRepository, WebsiteBowlerQueryRepository>();
            services.AddScoped<IWebsiteTitleQueryRepository, WebsiteTitleQueryRepository>();
            services.AddScoped<IWebsiteAwardQueryRepository, WebsiteAwardQueryRepository>();
            services.AddScoped<IWebsiteBowlingCenterQueryRepository, WebsiteBowlingCenterQueryRepository>();
            services.AddScoped<IWebsiteTournamentQueryRepository, WebsiteTournamentQueryRepository>();

            return services;
        }
    }
}
