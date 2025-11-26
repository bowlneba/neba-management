using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Infrastructure.Database.Website;

namespace Neba.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure services in the dependency injection container.
/// </summary>
public static class InfrastructureDependencyInjection
{
    #pragma warning disable S2325 // Extension methods should be static
    #pragma warning disable CA1034 // Nested types should not be visible

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the infrastructure services to the dependency injection container.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public IServiceCollection AddInfrastructure(IConfiguration config)
            => services
                .AddDatabase(config);

        internal IServiceCollection AddDatabase(IConfiguration config)
        {
            string? connectionString = config.GetConnectionString("bowlneba");

            services.AddDbContext<WebsiteDbContext>(options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, WebsiteDbContext.DefaultSchema)));

            return services;
        }
    }
}
