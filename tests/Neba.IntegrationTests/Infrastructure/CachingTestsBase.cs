using Microsoft.Extensions.DependencyInjection;
using Neba.Tests.Website;
using Neba.Website.Infrastructure.Database;

namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for caching-specific integration tests.
/// Unlike ApiTestsBase, this applies the caching decorator to test the caching functionality.
/// </summary>
public abstract class CachingTestsBase
    : IAsyncLifetime
{
    protected WebsiteDatabase Database { get; private set; } = null!;
    protected NebaCachingWebApplicationFactory Factory { get; private set; } = null!;

    public virtual async ValueTask InitializeAsync()
    {
        Database = new WebsiteDatabase();
        await Database.InitializeAsync();

        Factory = new NebaCachingWebApplicationFactory(Database);
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await Database.DisposeAsync();
    }

    private protected async Task SeedAsync(Func<WebsiteDbContext, Task> seedAction)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        WebsiteDbContext context = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        await seedAction(context);
        await context.SaveChangesAsync();
    }

    private protected async Task<TResult> ExecuteAsync<TResult>(Func<WebsiteDbContext, Task<TResult>> operation)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        WebsiteDbContext context = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        return await operation(context);
    }
}
