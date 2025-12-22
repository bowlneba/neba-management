using Microsoft.EntityFrameworkCore;
using Neba.Website.Infrastructure.Database;
using Testcontainers.PostgreSql;

namespace Neba.Tests.Website;

/// <summary>
/// Provides a PostgreSQL test database using Testcontainers for repository and integration tests.
/// Each test class creates its own isolated database instance for true parallel execution.
/// </summary>
public sealed class WebsiteDatabase
    : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17.6")
        .WithDatabase("website")
        .WithUsername("neba")
        .WithPassword("neba")
        .Build();

    /// <summary>
    /// Gets the connection string for the test database.
    /// </summary>
    public string ConnectionString
        => _container.GetConnectionString();

    /// <summary>
    /// Starts the PostgreSQL container and ensures the database schema is created.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        // Apply migrations to create a database schema
        await using var context = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(ConnectionString)
                .Options);
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Stops and disposes the PostgreSQL container.
    /// </summary>
    public async ValueTask DisposeAsync()
        => await _container.DisposeAsync();
}
