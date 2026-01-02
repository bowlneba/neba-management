using Testcontainers.Azurite;

namespace Neba.Tests.Infrastructure;

/// <summary>
/// Manages the lifecycle of an Azurite container for integration testing Azure Storage functionality.
/// Follows the same pattern as DatabaseContainer for database testing.
/// </summary>
public sealed class AzureStorageContainer : IAsyncLifetime
{
    private readonly AzuriteContainer _container = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest")
        .Build();

    /// <summary>
    /// Gets the connection string for the Azurite blob service.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Starts the Azurite container asynchronously.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    /// <summary>
    /// Stops and disposes the Azurite container asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
