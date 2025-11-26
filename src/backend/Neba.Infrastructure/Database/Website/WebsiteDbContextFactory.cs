using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Neba.Infrastructure.Database.Website;

internal sealed class WebsiteDbContextFactory
    : IDesignTimeDbContextFactory<WebsiteDbContext>
{
    public WebsiteDbContext CreateDbContext(string[] args)
    {
        // Use standard ASP.NET Core configuration hierarchy (last source added wins):
        // 1. Environment variables (lowest priority - base)
        // 2. appsettings.json (can override environment)
        // 3. appsettings.development.json (highest priority - local development)
        // 4. Hardcoded fallback if all are empty
        string apiProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..",
            "..",
            "Neba.Api");

        string? connectionString = null;

        if (Directory.Exists(apiProjectPath))
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.development.json", optional: true)
                .Build();

            connectionString = configuration.GetConnectionString("Website");
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
