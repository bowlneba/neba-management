using System.Reflection;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application;
using Neba.Application.Messaging;
using Neba.Infrastructure.BackgroundJobs;
using Neba.Infrastructure.Caching;
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
        /// Configures the application to use infrastructure services, including caching,
        /// key vault, Google Docs integration, background jobs, document management,
        /// and storage services.
        /// </summary>
        /// <param name="config">The configuration manager providing application settings.</param>
        /// <param name="cachingAssemblies">Assemblies to scan for caching-related features.</param>
        /// <returns>The updated service collection for method chaining.</returns>
        public IServiceCollection AddInfrastructure(IConfigurationManager config, Assembly[] cachingAssemblies)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(cachingAssemblies);

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            return services
                .AddCaching(config, cachingAssemblies)
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

        private IServiceCollection AddCaching(IConfiguration config, Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                return services;
            }

            services.AddDistributedPostgresCache(options =>
            {
                options.ConnectionString = config.GetConnectionString("cache")
                    ?? throw new InvalidOperationException("Cache connection string is not configured.");

                options.SchemaName = "public";
                options.TableName = "distributed_cache";
                options.CreateIfNotExists = true;
            });

            services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = 1024 * 1024 * 10; // 10 MB
                options.MaximumKeyLength = 512;
                options.ReportTagMetrics = true;
            });

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Decorate(typeof(IQueryHandler<,>), typeof(CachedQueryHandlerDecorator<,>));

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
