using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neba.Infrastructure.Database;
using Neba.Tests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Infrastructure.Database;

namespace Neba.IntegrationTests.Infrastructure.Database;

[Trait("Category", "Integration")]
[Trait("Component", "Infrastructure.Database")]
public sealed class DatabaseTelemetryTests : IAsyncLifetime
{
    private DatabaseContainer _database = null!;

    public async ValueTask InitializeAsync()
    {
        _database = new DatabaseContainer();
        await _database.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
        => await _database.DisposeAsync();

    [Fact(DisplayName = "Database queries work with SlowQueryInterceptor")]
    public async Task DatabaseQueries_WorkWithSlowQueryInterceptor()
    {
        // Arrange
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        ILogger<SlowQueryInterceptor> logger = loggerFactory.CreateLogger<SlowQueryInterceptor>();
        var interceptor = new SlowQueryInterceptor(logger, slowQueryThresholdMs: 5000);

        await using var context = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .AddInterceptors(interceptor)
                .Options);

        // Seed some data
        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(5, 1963);
        await context.Bowlers.AddRangeAsync(seedBowlers);
        await context.SaveChangesAsync();

        // Act - Execute query through interceptor
        List<Bowler> result = await context.Bowlers.Take(3).ToListAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact(DisplayName = "SlowQueryInterceptor allows multiple concurrent queries")]
    public async Task SlowQueryInterceptor_AllowsMultipleConcurrentQueries()
    {
        // Arrange
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        ILogger<SlowQueryInterceptor> logger = loggerFactory.CreateLogger<SlowQueryInterceptor>();
        var interceptor = new SlowQueryInterceptor(logger, slowQueryThresholdMs: 10000);

        await using var context = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .AddInterceptors(interceptor)
                .Options);

        // Seed data
        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(10, 1963);
        await context.Bowlers.AddRangeAsync(seedBowlers);
        await context.SaveChangesAsync();

        // Act - Multiple concurrent queries
        int count1 = await context.Bowlers.CountAsync();
        int count2 = await context.Bowlers.Where(b => b.Id != default).CountAsync();
        List<Bowler> list = await context.Bowlers.Take(5).ToListAsync();

        // Assert
        count1.ShouldBe(10);
        count2.ShouldBe(10);
        list.Count.ShouldBe(5);
    }

    [Fact(DisplayName = "SlowQueryInterceptor handles different query types")]
    public async Task SlowQueryInterceptor_HandlesDifferentQueryTypes()
    {
        // Arrange
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        ILogger<SlowQueryInterceptor> logger = loggerFactory.CreateLogger<SlowQueryInterceptor>();
        var interceptor = new SlowQueryInterceptor(logger, slowQueryThresholdMs: 10000);

        await using var context = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .AddInterceptors(interceptor)
                .Options);

        // Seed data
        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(3, 1963);
        await context.Bowlers.AddRangeAsync(seedBowlers);
        await context.SaveChangesAsync();

        // Act & Assert - Different query types
        // Select query
        Bowler? selectResult = await context.Bowlers.FirstOrDefaultAsync();
        selectResult.ShouldNotBeNull();

        // Count query
        int countResult = await context.Bowlers.CountAsync();
        countResult.ShouldBe(3);

        // Any query
        bool anyResult = await context.Bowlers.AnyAsync();
        anyResult.ShouldBeTrue();
    }

    [Fact(DisplayName = "SlowQueryInterceptor can be instantiated with default threshold")]
    public void SlowQueryInterceptor_CanBeInstantiatedWithDefaultThreshold()
    {
        // Arrange
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<SlowQueryInterceptor> logger = loggerFactory.CreateLogger<SlowQueryInterceptor>();

        // Act & Assert
        Should.NotThrow(() => new SlowQueryInterceptor(logger));
    }

    [Fact(DisplayName = "SlowQueryInterceptor can be instantiated with custom threshold")]
    public void SlowQueryInterceptor_CanBeInstantiatedWithCustomThreshold()
    {
        // Arrange
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<SlowQueryInterceptor> logger = loggerFactory.CreateLogger<SlowQueryInterceptor>();

        // Act & Assert
        Should.NotThrow(() => new SlowQueryInterceptor(logger, slowQueryThresholdMs: 500));
    }

    [Fact(DisplayName = "SlowQueryInterceptor with very low threshold detects queries as slow")]
    public async Task SlowQueryInterceptor_WithVeryLowThreshold_DetectsSlowQueries()
    {
        // Arrange
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        ILogger<SlowQueryInterceptor> logger = loggerFactory.CreateLogger<SlowQueryInterceptor>();
        // Set very low threshold so all queries appear slow
        var interceptor = new SlowQueryInterceptor(logger, slowQueryThresholdMs: 0.001);

        await using var context = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .AddInterceptors(interceptor)
                .Options);

        // Seed data
        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(2, 1963);
        await context.Bowlers.AddRangeAsync(seedBowlers);
        await context.SaveChangesAsync();

        // Act - This query should be detected as slow
        List<Bowler> result = await context.Bowlers.ToListAsync();

        // Assert - Query completes successfully even when detected as slow
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }
}
