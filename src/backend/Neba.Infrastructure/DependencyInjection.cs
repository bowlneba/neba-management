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
        public IServiceCollection AddInfrastructure(IConfigurationManager config)
            => services
                .AddKeyVault(config)
                .AddDatabase(config);

        internal IServiceCollection AddDatabase(IConfigurationManager config)
        {
            string bowlnebaConnectionString = config.GetConnectionString("bowlneba")
                ?? throw new InvalidOperationException("Database connection string 'bowlneba' is not configured.");

            services.AddDbContext<WebsiteDbContext>(options => options
                .UseNpgsql(bowlnebaConnectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, WebsiteDbContext.DefaultSchema)));

            return services;
        }

        internal IServiceCollection AddKeyVault(IConfigurationManager config)
        {
#if DEBUG
            // In debug builds, we do not want to connect to Key Vault.
            return services;
#else
            KeyVaultOptions keyVaultOptions = new();
            config.GetSection("KeyVault").Bind(keyVaultOptions);

            config.AddAzureKeyVault(keyVaultOptions.VaultUrl, new DefaultAzureCredential());

            return services;
#endif
        }
    }
}
