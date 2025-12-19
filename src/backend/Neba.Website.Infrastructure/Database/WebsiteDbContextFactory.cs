using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Neba.Website.Infrastructure.Database;

internal sealed class WebsiteDbContextFactory
    : IDesignTimeDbContextFactory<WebsiteDbContext>
{
    public WebsiteDbContext CreateDbContext(string[] args)
    {
        // Priority order (highest to lowest):
        // 1. Environment variables (for CI/CD and production)
        // 2. appsettings.Development.json (for local development)
        // 3. appsettings.json
        // 4. Hardcoded fallback

        // First, try to read from environment variable (for GitHub Actions / Azure)
        string? connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__bowlneba");

        // If not in environment, try configuration files from API project
        if (string.IsNullOrEmpty(connectionString))
        {
            string apiProjectPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "..",
                "Neba.Api");

            if (Directory.Exists(apiProjectPath))
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(apiProjectPath)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();

                connectionString = configuration.GetConnectionString("website-migrations");
            }
        }

        DbContextOptionsBuilder<WebsiteDbContext> optionsBuilder = new();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                WebsiteDbContext.DefaultSchema));

        return new WebsiteDbContext(optionsBuilder.Options);
    }
}
