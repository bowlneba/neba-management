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

    /// <summary>
    /// Gets the connection string for the test database.
    /// </summary>
    public string ConnectionString
        => _container.GetConnectionString();

    /// <summary>
    /// Starts the PostgreSQL container and ensures the database schema is created.
    /// </summary>
    public async ValueTask InitializeAsync()
        => await _container.StartAsync();

    /// <summary>
    /// Stops and disposes the PostgreSQL container.
    /// </summary>
    public async ValueTask DisposeAsync()
        => await _container.DisposeAsync();
}
