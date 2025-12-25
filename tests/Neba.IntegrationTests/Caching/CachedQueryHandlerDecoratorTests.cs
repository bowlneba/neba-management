using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Messaging;
using Neba.IntegrationTests.Infrastructure;

namespace Neba.IntegrationTests.Caching;

public sealed class CachedQueryHandlerDecoratorTests : CachingTestsBase
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        // Clear all cached data from previous test runs using wildcard tag
        // This invalidates ALL HybridCache entries, ensuring test isolation
        HybridCache cache = Factory.Services.GetRequiredService<HybridCache>();
        await cache.RemoveByTagAsync("*");

        // Clear static state from previous test runs to prevent test pollution
        TestCachedQueryHandler.InvocationCounts.Clear();
        TestCachedQueryWithTagsHandler.InvocationCounts.Clear();
        TestCachedQueryWithExpiryHandler.InvocationCounts.Clear();
    }

    [Fact]
    public async Task CachedQuery_FirstCall_ShouldExecuteHandler()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestCachedQuery, TestResponse> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TestCachedQuery, TestResponse>>();

        var query = new TestCachedQuery("test-key-1");

        // Act
        TestResponse result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe("Response for test-key-1");
        result.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task CachedQuery_MultipleCalls_ShouldUseCacheAndCallHandlerOnce()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestCachedQuery, TestResponse> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TestCachedQuery, TestResponse>>();

        var query = new TestCachedQuery("test-key-2");

        // Act - Call the handler 5 times with the same query
        TestResponse result1 = await handler.HandleAsync(query, CancellationToken.None);
        TestResponse result2 = await handler.HandleAsync(query, CancellationToken.None);
        TestResponse result3 = await handler.HandleAsync(query, CancellationToken.None);
        TestResponse result4 = await handler.HandleAsync(query, CancellationToken.None);
        TestResponse result5 = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - All results should be identical
        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result3.ShouldNotBeNull();
        result4.ShouldNotBeNull();
        result5.ShouldNotBeNull();

        result1.Value.ShouldBe("Response for test-key-2");
        result2.Value.ShouldBe("Response for test-key-2");
        result3.Value.ShouldBe("Response for test-key-2");
        result4.Value.ShouldBe("Response for test-key-2");
        result5.Value.ShouldBe("Response for test-key-2");

        // The handler should only be invoked once (cache miss)
        // All subsequent calls should be cache hits
        result1.InvocationCount.ShouldBe(1);
        result2.InvocationCount.ShouldBe(1);
        result3.InvocationCount.ShouldBe(1);
        result4.InvocationCount.ShouldBe(1);
        result5.InvocationCount.ShouldBe(1);

        // Verify the invocation count is tracked correctly
        TestCachedQueryHandler.InvocationCounts["test-key-2"].ShouldBe(1);
    }

    [Fact]
    public async Task CachedQuery_DifferentKeys_ShouldCacheSeparately()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestCachedQuery, TestResponse> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TestCachedQuery, TestResponse>>();

        var query1 = new TestCachedQuery("test-key-3a");
        var query2 = new TestCachedQuery("test-key-3b");

        // Act - Call the handler with different cache keys
        TestResponse result1a = await handler.HandleAsync(query1, CancellationToken.None);
        TestResponse result1b = await handler.HandleAsync(query1, CancellationToken.None);

        TestResponse result2a = await handler.HandleAsync(query2, CancellationToken.None);
        TestResponse result2b = await handler.HandleAsync(query2, CancellationToken.None);

        // Assert - Each unique key should result in one handler invocation
        result1a.Value.ShouldBe("Response for test-key-3a");
        result1b.Value.ShouldBe("Response for test-key-3a");
        result2a.Value.ShouldBe("Response for test-key-3b");
        result2b.Value.ShouldBe("Response for test-key-3b");

        TestCachedQueryHandler.InvocationCounts["test-key-3a"].ShouldBe(1);
        TestCachedQueryHandler.InvocationCounts["test-key-3b"].ShouldBe(1);
    }

    [Fact]
    public async Task CachedQuery_WithTags_ShouldCacheWithTags()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestCachedQueryWithTags, TestResponse> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TestCachedQueryWithTags, TestResponse>>();

        var query = new TestCachedQueryWithTags("test-key-4", ["tag1", "tag2"]);

        // Act
        TestResponse result1 = await handler.HandleAsync(query, CancellationToken.None);
        TestResponse result2 = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - Should be cached
        result1.Value.ShouldBe("Response for test-key-4");
        result2.Value.ShouldBe("Response for test-key-4");
        TestCachedQueryWithTagsHandler.InvocationCounts["test-key-4"].ShouldBe(1);
    }

    [Fact]
    public async Task CachedQuery_WithCustomExpiry_ShouldRespectExpiry()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestCachedQueryWithExpiry, TestResponse> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TestCachedQueryWithExpiry, TestResponse>>();

        var query = new TestCachedQueryWithExpiry("test-key-5");

        // Act
        TestResponse result1 = await handler.HandleAsync(query, CancellationToken.None);
        TestResponse result2 = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - Should be cached
        result1.Value.ShouldBe("Response for test-key-5");
        result2.Value.ShouldBe("Response for test-key-5");
        TestCachedQueryWithExpiryHandler.InvocationCounts["test-key-5"].ShouldBe(1);
    }
}

// Test query that implements ICachedQuery
public sealed record TestCachedQuery(string CacheKey) : ICachedQuery<TestResponse>
{
    public string Key => CacheKey;
}

// Test response that tracks invocation count
public sealed record TestResponse(string Value, int InvocationCount);

// Test handler that tracks how many times it's invoked
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - instantiated by DI
internal sealed class TestCachedQueryHandler : IQueryHandler<TestCachedQuery, TestResponse>
#pragma warning restore CA1812
{
    // Static dictionary to track invocations across decorator boundaries
    public static readonly Dictionary<string, int> InvocationCounts = [];

    public Task<TestResponse> HandleAsync(TestCachedQuery query, CancellationToken cancellationToken)
    {
        // Track the invocation
        if (!InvocationCounts.TryGetValue(query.CacheKey, out int currentCount))
        {
            currentCount = 0;
        }

        currentCount++;
        InvocationCounts[query.CacheKey] = currentCount;

        var response = new TestResponse($"Response for {query.CacheKey}", currentCount);
        return Task.FromResult(response);
    }
}

// Test query with custom tags
public sealed record TestCachedQueryWithTags(string CacheKey, IReadOnlyCollection<string> CacheTags)
    : ICachedQuery<TestResponse>
{
    public string Key => CacheKey;
    public IReadOnlyCollection<string> Tags => CacheTags;
}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes - instantiated by DI
internal sealed class TestCachedQueryWithTagsHandler : IQueryHandler<TestCachedQueryWithTags, TestResponse>
#pragma warning restore CA1812
{
    public static readonly Dictionary<string, int> InvocationCounts = [];

    public Task<TestResponse> HandleAsync(TestCachedQueryWithTags query, CancellationToken cancellationToken)
    {
        if (!InvocationCounts.TryGetValue(query.CacheKey, out int currentCount))
        {
            currentCount = 0;
        }

        currentCount++;
        InvocationCounts[query.CacheKey] = currentCount;

        var response = new TestResponse($"Response for {query.CacheKey}", currentCount);
        return Task.FromResult(response);
    }
}

// Test query with custom expiry
public sealed record TestCachedQueryWithExpiry(string CacheKey) : ICachedQuery<TestResponse>
{
    public string Key => CacheKey;
    public TimeSpan Expiry => TimeSpan.FromMinutes(5);
}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes - instantiated by DI
internal sealed class TestCachedQueryWithExpiryHandler : IQueryHandler<TestCachedQueryWithExpiry, TestResponse>
#pragma warning restore CA1812
{
    public static readonly Dictionary<string, int> InvocationCounts = [];

    public Task<TestResponse> HandleAsync(TestCachedQueryWithExpiry query, CancellationToken cancellationToken)
    {
        if (!InvocationCounts.TryGetValue(query.CacheKey, out int currentCount))
        {
            currentCount = 0;
        }

        currentCount++;
        InvocationCounts[query.CacheKey] = currentCount;

        var response = new TestResponse($"Response for {query.CacheKey}", currentCount);
        return Task.FromResult(response);
    }
}
