using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Infrastructure.BackgroundJobs;
using Neba.Infrastructure.Documents;
using Neba.Infrastructure.Storage;

namespace Neba.Infrastructure;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable CA1708 // Identifiers should differ by more than case

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
                .AddCaching()
                .AddKeyVault(config)
                .AddGoogleDocs(config)
                .AddBackgroundJobs(config)
                .AddDocumentBackgroundJobs()
                .AddDocumentRefreshNotification()
                .AddStorageService(config);
        }

        private IServiceCollection AddKeyVault(IConfigurationManager config)
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
                            .AddSecret("ConnectionStrings--website-migrations")
                            .AddSecret("ConnectionStrings--website")
                            .AddSecret("ConnectionStrings--hangfire")
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

        private IServiceCollection AddCaching()
        {
            services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = 1024 * 1024 * 10; // 10 MB
                options.MaximumKeyLength = 512;
                options.ReportTagMetrics = true;
            });

            return services;
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Configures the application to use the infrastructure services.
        /// </summary>
        /// <returns>The web application for method chaining.</returns>
        public WebApplication UseInfrastructure()
        {
            app.UseBackgroundJobsDashboard();

            return app;
        }
    }
}
