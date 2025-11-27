using Microsoft.EntityFrameworkCore;
using Neba.Infrastructure.Database.Website;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Neba.Tests;

/// <summary>
/// Provides a PostgreSQL test database using Testcontainers for repository and integration tests.
/// Implements IAsyncLifetime to ensure the container is started before tests run.
/// </summary>
public sealed class WebsiteDatabase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17.6")
        .WithDatabase("bowlneba")
        .WithUsername("neba")
        .WithPassword("neba")
        .Build();

    private Respawner _respawner = null!;

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

        // Apply migrations to create database schema
        await using var context = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(ConnectionString)
                .Options);
        await context.Database.MigrateAsync();

        // Initialize Respawner
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["website"]
        });
    }

    /// <summary>
    /// Resets the database to a clean state by deleting all data from tables.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Stops and disposes the PostgreSQL container.
    /// </summary>
    public async ValueTask DisposeAsync()
        => await _container.DisposeAsync();
}
