using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Bowlers;
using Neba.Infrastructure.Database.Website;
using Neba.Infrastructure.Database.Website.Repositories;

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
        /// <param name="config">The configuration manager containing application settings.</param>
        /// <returns>The service collection for method chaining.</returns>
        public IServiceCollection AddInfrastructure(IConfigurationManager config)
        {
            ArgumentNullException.ThrowIfNull(config);

            return services
                .AddKeyVault(config)
                .AddDatabase(config);
        }

        internal IServiceCollection AddDatabase(IConfigurationManager config)
        {
            string bowlnebaConnectionString = config.GetConnectionString("bowlneba")
                ?? throw new InvalidOperationException("Database connection string 'bowlneba' is not configured.");

            services.AddDbContext<WebsiteDbContext>(options => options
                .UseNpgsql(bowlnebaConnectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, WebsiteDbContext.DefaultSchema)));

            string[] bowlnebaTags = ["database", "bowlneba"];

            services.AddHealthChecks()
                .AddDbContextCheck<WebsiteDbContext>(
                    name: "Bowlneba Database",
                    tags: bowlnebaTags);

            services.AddRepositories();

            return services;
        }

        internal IServiceCollection AddRepositories()
        {
            services.AddScoped<IWebsiteBowlerQueryRepository, WebsiteBowlerQueryRepository>();

            return services;
        }

        internal IServiceCollection AddKeyVault(IConfigurationManager config)
        {
            // Only connect to Key Vault if explicitly enabled via configuration
            // This allows Release builds without Key Vault (e.g., Docker local development)
            // while still supporting Key Vault in actual production/staging deployments
            bool useKeyVault = config.GetValue("KeyVault:Enabled", false);

            if (!useKeyVault)
            {
                return services;
            }

            string? vaultUrl = config["KeyVault:VaultUrl"];
            if (string.IsNullOrEmpty(vaultUrl))
            {
                throw new InvalidOperationException("KeyVault:VaultUrl is not configured but KeyVault is enabled.");
            }

            var vaultUri = new Uri(vaultUrl);
            var defaultAzureCredential = new DefaultAzureCredential();

            config.AddAzureKeyVault(vaultUri, defaultAzureCredential);

            string[] keyVaultTags = ["keyvault", "azure"];
            services.AddHealthChecks()
                .AddAzureKeyVault(vaultUri, defaultAzureCredential,
                    _ =>
                    {
                        // This is where specific secret/key checks would go
                    },
                    name: "Azure Key Vault",
                    tags: keyVaultTags);

            return services;
        }
    }
}
