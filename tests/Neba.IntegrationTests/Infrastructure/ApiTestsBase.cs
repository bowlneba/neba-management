using Microsoft.Extensions.DependencyInjection;
using Neba.Infrastructure.Database.Website;
using Neba.Tests;
using Neba.Tests.Website;

namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that provides a test database and web application factory.
/// Each test class that inherits from this will get its own isolated database instance.
/// </summary>
public abstract class ApiTestsBase
    : IAsyncLifetime
{
    /// <summary>
    /// Gets the test database instance for this test class.
    /// </summary>
    protected WebsiteDatabase Database { get; private set; } = null!;

    /// <summary>
    /// Gets the web application factory for creating HTTP clients.
    /// </summary>
    protected NebaWebApplicationFactory Factory { get; private set; } = null!;

    /// <summary>
    /// Initializes the test database and web application factory before any tests run.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        Database = new WebsiteDatabase();
        await Database.InitializeAsync();

        Factory = new NebaWebApplicationFactory(Database);
    }

    /// <summary>
    /// Cleans up the database and factory after all tests have completed.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await Database.DisposeAsync();
    }

    /// <summary>
    /// Resets the database to a clean state between tests.
    /// Call this method in test setup or at the start of each test to ensure isolation.
    /// </summary>
    protected async Task ResetDatabaseAsync()
        => await Database.ResetAsync();

    /// <summary>
    /// Executes an action with a scoped DbContext for seeding data.
    /// The context is automatically saved and disposed after the action completes.
    /// </summary>
    /// <param name="seedAction">The action to perform with the DbContext.</param>
    /// <example>
    /// <code>
    /// await SeedAsync(async context =>
    /// {
    ///     context.Bowlers.Add(new Bowler { ... });
    /// });
    /// </code>
    /// </example>
    private protected async Task SeedAsync(Func<WebsiteDbContext, Task> seedAction)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        WebsiteDbContext context = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        await seedAction(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes a function with a scoped DbContext and returns a result.
    /// Useful for querying data or performing operations that need to return values.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="operation">The operation to perform with the DbContext.</param>
    /// <returns>The result of the operation.</returns>
    /// <example>
    /// <code>
    /// int bowlerCount = await ExecuteAsync(async context =>
    ///     await context.Bowlers.CountAsync());
    /// </code>
    /// </example>
    private protected async Task<TResult> ExecuteAsync<TResult>(Func<WebsiteDbContext, Task<TResult>> operation)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        WebsiteDbContext context = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        return await operation(context);
    }
}
