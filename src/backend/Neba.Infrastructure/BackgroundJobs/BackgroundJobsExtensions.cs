using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.BackgroundJobs;

namespace Neba.Infrastructure.BackgroundJobs;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Unused private types or members should be removed

internal static class BackgroundJobsExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBackgroundJobs(IConfiguration config)
        {
            services.AddOptions<HangfireSettings>()
                .Bind(config.GetSection("Hangfire"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton(sp =>
            {
                HangfireSettings settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<HangfireSettings>>().Value;

                return settings;
            });

            services.AddHealthChecks()
                .AddHangfire(options => options.MinimumAvailableServers = 1,
                name: "Background Jobs",
                tags: ["infrastructure", "background-jobs"]);

            services.AddHangfireInfrastructure(config);

            services.AddScoped<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();

            return services;
        }

        private IServiceCollection AddHangfireInfrastructure(IConfiguration config)
        {
            string hangfireConnectionString = config.GetConnectionString("hangfire")
                ?? throw new InvalidOperationException("Hangfire connection string is not configured.");

            services.AddHangfire((serviceProvider, options) =>
            {
                HangfireSettings settings = serviceProvider.GetRequiredService<HangfireSettings>();

                options
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseFilter(new AutomaticRetryAttribute { Attempts = 3 })
                    .UseFilter(new JobExpirationFilterAttribute(settings))
                    .UsePostgreSqlStorage(postgres => postgres
                        .UseNpgsqlConnection(hangfireConnectionString),
                        new PostgreSqlStorageOptions
                        {
                            SchemaName = "hangfire",
                            PrepareSchemaIfNecessary = true,
                            EnableTransactionScopeEnlistment = true,
                            DeleteExpiredBatchSize = 1000,
                            QueuePollInterval = TimeSpan.FromSeconds(30),
                            JobExpirationCheckInterval = TimeSpan.FromHours(1),
                            CountersAggregateInterval = TimeSpan.FromMinutes(5),
                            TransactionSynchronisationTimeout = TimeSpan.FromMinutes(1)
                        });
            });

            services.AddHangfireServer((serviceProvider, options) =>
            {
                HangfireSettings settings = serviceProvider.GetRequiredService<HangfireSettings>();

                options.WorkerCount = settings.WorkerCount;
                options.ServerName = $"API - {Environment.MachineName}";
                options.Queues = ["default"];
            });

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseBackgroundJobsDashboard()
        {
            // Only register the dashboard if Hangfire is configured
            // (allows tests to remove Hangfire services without breaking middleware)
            if (app.Services.GetService(typeof(JobStorage)) is not null)
            {
                app.UseHangfireDashboard("/background-jobs", new DashboardOptions
                {
                    Authorization = [new BackgroundJobDashboardAuthorizationFilter()],
                    DashboardTitle = "Background Jobs - API",
                    StatsPollingInterval = 5000,
                    DisplayStorageConnectionString = false,
                    IsReadOnlyFunc = _ => false
                });
            }

            return app;
        }
    }
}
