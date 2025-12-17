using Azure.Core;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Awards;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Infrastructure.Database.Website;
using Neba.Infrastructure.Database.Website.Repositories;
using Neba.Infrastructure.Documents;
using Npgsql;

namespace Neba.Infrastructure;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S1144 // Remove unused constructor of private type.

/// <summary>
/// Provides extension methods for registering infrastructure services in the dependency injection container.
/// </summary>
public static class InfrastructureDependencyInjection
{
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
                .AddDatabase(config)
                .AddGoogleDocs(config);
        }

        internal IServiceCollection AddDatabase(IConfigurationManager config)
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

        internal IServiceCollection AddRepositories()
        {
            services.AddScoped<IWebsiteBowlerQueryRepository, WebsiteBowlerQueryRepository>();
            services.AddScoped<IWebsiteTitleQueryRepository, WebsiteTitleQueryRepository>();
            services.AddScoped<IWebsiteAwardQueryRepository, WebsiteAwardQueryRepository>();

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
                    options =>
                    {
                        options
                            .AddSecret("ConnectionStrings--bowlneba")
                            .AddSecret("GoogleDocs--Credentials--ClientEmail")
                            .AddSecret("GoogleDocs--Credentials--ClientId")
                            .AddSecret("GoogleDocs--Credentials--PrivateKeyId")
                            .AddSecret("GoogleDocs--Credentials--PrivateKey")
                            .AddSecret("GoogleDocs--Credentials--ClientX509CertUrl");
                    },
                    name: "Azure Key Vault",
                    tags: keyVaultTags);

            return services;
        }
    }
}
