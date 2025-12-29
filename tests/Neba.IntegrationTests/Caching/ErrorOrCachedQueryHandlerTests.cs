using ErrorOr;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Messaging;
using Neba.IntegrationTests.Infrastructure;

namespace Neba.IntegrationTests.Caching;

public sealed class ErrorOrCachedQueryHandlerTests : CachingTestsBase
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        // Clear all cached data from previous test runs
        HybridCache cache = Factory.Services.GetRequiredService<HybridCache>();
        await cache.RemoveByTagAsync("*");

        // Clear static state from previous test runs
        TestErrorOrQueryHandler.InvocationCounts.Clear();
        TestErrorOrQueryHandler.ReturnErrors.Clear();
    }

    [Fact]
    public async Task ErrorOrQuery_FirstCallWithSuccess_ShouldExecuteHandlerAndCacheUnwrappedValue()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>>();

        var query = new TestErrorOrQuery("test-key-1");

        // Configure handler to return success
        TestErrorOrQueryHandler.ReturnErrors["test-key-1"] = false;

        // Act
        ErrorOr<TestDto> result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.Value.ShouldBe("Response for test-key-1");
        result.Value.InvocationCount.ShouldBe(1);

        // Handler should have been called once
        TestErrorOrQueryHandler.InvocationCounts["test-key-1"].ShouldBe(1);
    }

    [Fact]
    public async Task ErrorOrQuery_SecondCallWithSuccess_ShouldReturnCachedRewrappedValue()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>>();

        var query = new TestErrorOrQuery("test-key-2");
        TestErrorOrQueryHandler.ReturnErrors["test-key-2"] = false;

        // Act - Call twice
        ErrorOr<TestDto> result1 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result2 = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - Both should be successful
        result1.IsError.ShouldBeFalse();
        result1.Value.ShouldNotBeNull();
        result1.Value.Value.ShouldBe("Response for test-key-2");

        result2.IsError.ShouldBeFalse();
        result2.Value.ShouldNotBeNull();
        result2.Value.Value.ShouldBe("Response for test-key-2");

        // Both should have invocation count of 1 (from cache)
        result1.Value.InvocationCount.ShouldBe(1);
        result2.Value.InvocationCount.ShouldBe(1);

        // Handler should only be called once (second call is cache hit)
        TestErrorOrQueryHandler.InvocationCounts["test-key-2"].ShouldBe(1);
    }

    [Fact]
    public async Task ErrorOrQuery_CallWithError_ShouldNotCache()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>>();

        var query = new TestErrorOrQuery("test-key-3");

        // Configure handler to return error
        TestErrorOrQueryHandler.ReturnErrors["test-key-3"] = true;

        // Act
        ErrorOr<TestDto> result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Test.NotFound");
        result.FirstError.Description.ShouldBe("Not found: test-key-3");

        // Handler should have been called once
        TestErrorOrQueryHandler.InvocationCounts["test-key-3"].ShouldBe(1);
    }

    [Fact]
    public async Task ErrorOrQuery_RepeatedErrorCalls_ShouldAlwaysExecuteHandler()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>>();

        var query = new TestErrorOrQuery("test-key-4");
        TestErrorOrQueryHandler.ReturnErrors["test-key-4"] = true;

        // Act - Call multiple times with error
        ErrorOr<TestDto> result1 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result2 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result3 = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - All should be errors
        result1.IsError.ShouldBeTrue();
        result2.IsError.ShouldBeTrue();
        result3.IsError.ShouldBeTrue();

        // Handler should be called 3 times (errors not cached)
        TestErrorOrQueryHandler.InvocationCounts["test-key-4"].ShouldBe(3);
    }

    [Fact]
    public async Task ErrorOrQuery_MixedSuccessAndError_ShouldOnlyCacheSuccess()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>>();

        var query = new TestErrorOrQuery("test-key-5");

        // Act - First call succeeds
        TestErrorOrQueryHandler.ReturnErrors["test-key-5"] = false;
        ErrorOr<TestDto> result1 = await handler.HandleAsync(query, CancellationToken.None);

        // Second and third calls should get cached success (handler not called)
        ErrorOr<TestDto> result2 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result3 = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - All should be successful (from cache)
        result1.IsError.ShouldBeFalse();
        result1.Value.Value.ShouldBe("Response for test-key-5");

        result2.IsError.ShouldBeFalse();
        result2.Value.Value.ShouldBe("Response for test-key-5");

        result3.IsError.ShouldBeFalse();
        result3.Value.Value.ShouldBe("Response for test-key-5");

        // Handler should only be called once (first call)
        TestErrorOrQueryHandler.InvocationCounts["test-key-5"].ShouldBe(1);
    }

    [Fact]
    public async Task ErrorOrQuery_MultipleCalls_ShouldCacheAndRewrapCorrectly()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>>();

        var query = new TestErrorOrQuery("test-key-6");
        TestErrorOrQueryHandler.ReturnErrors["test-key-6"] = false;

        // Act - Call 5 times
        ErrorOr<TestDto> result1 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result2 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result3 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result4 = await handler.HandleAsync(query, CancellationToken.None);
        ErrorOr<TestDto> result5 = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - All should be successful with same value
        result1.IsError.ShouldBeFalse();
        result2.IsError.ShouldBeFalse();
        result3.IsError.ShouldBeFalse();
        result4.IsError.ShouldBeFalse();
        result5.IsError.ShouldBeFalse();

        result1.Value.Value.ShouldBe("Response for test-key-6");
        result2.Value.Value.ShouldBe("Response for test-key-6");
        result3.Value.Value.ShouldBe("Response for test-key-6");
        result4.Value.Value.ShouldBe("Response for test-key-6");
        result5.Value.Value.ShouldBe("Response for test-key-6");

        // All should have same invocation count (from cache)
        result1.Value.InvocationCount.ShouldBe(1);
        result2.Value.InvocationCount.ShouldBe(1);
        result3.Value.InvocationCount.ShouldBe(1);
        result4.Value.InvocationCount.ShouldBe(1);
        result5.Value.InvocationCount.ShouldBe(1);

        // Handler should only be called once
        TestErrorOrQueryHandler.InvocationCounts["test-key-6"].ShouldBe(1);
    }

    [Fact]
    public async Task ErrorOrQuery_DifferentKeys_ShouldCacheSeparately()
    {
        // Arrange
        using IServiceScope scope = Factory.Services.CreateScope();
        IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>>();

        var query1 = new TestErrorOrQuery("test-key-7a");
        var query2 = new TestErrorOrQuery("test-key-7b");

        TestErrorOrQueryHandler.ReturnErrors["test-key-7a"] = false;
        TestErrorOrQueryHandler.ReturnErrors["test-key-7b"] = false;

        // Act
        ErrorOr<TestDto> result1a = await handler.HandleAsync(query1, CancellationToken.None);
        ErrorOr<TestDto> result1b = await handler.HandleAsync(query1, CancellationToken.None);

        ErrorOr<TestDto> result2a = await handler.HandleAsync(query2, CancellationToken.None);
        ErrorOr<TestDto> result2b = await handler.HandleAsync(query2, CancellationToken.None);

        // Assert
        result1a.Value.Value.ShouldBe("Response for test-key-7a");
        result1b.Value.Value.ShouldBe("Response for test-key-7a");
        result2a.Value.Value.ShouldBe("Response for test-key-7b");
        result2b.Value.Value.ShouldBe("Response for test-key-7b");

        // Each key should result in one handler invocation
        TestErrorOrQueryHandler.InvocationCounts["test-key-7a"].ShouldBe(1);
        TestErrorOrQueryHandler.InvocationCounts["test-key-7b"].ShouldBe(1);
    }
}

// Test DTO
public sealed record TestDto(string Value, int InvocationCount);

// Test query that implements ICachedQuery and returns ErrorOr<TestDto>
public sealed record TestErrorOrQuery(string CacheKey) : ICachedQuery<ErrorOr<TestDto>>
{
    public string Key => CacheKey;
}

// Test handler that tracks invocations and can return errors or success
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - instantiated by DI
internal sealed class TestErrorOrQueryHandler : IQueryHandler<TestErrorOrQuery, ErrorOr<TestDto>>
#pragma warning restore CA1812
{
    // Static dictionary to track invocations across decorator boundaries
    public static readonly Dictionary<string, int> InvocationCounts = [];

    // Static dictionary to control whether to return error or success
    public static readonly Dictionary<string, bool> ReturnErrors = [];

    public Task<ErrorOr<TestDto>> HandleAsync(TestErrorOrQuery query, CancellationToken cancellationToken)
    {
        // Track the invocation
        if (!InvocationCounts.TryGetValue(query.CacheKey, out int currentCount))
        {
            currentCount = 0;
        }

        currentCount++;
        InvocationCounts[query.CacheKey] = currentCount;

        // Check if we should return an error
        if (ReturnErrors.TryGetValue(query.CacheKey, out bool shouldError) && shouldError)
        {
            ErrorOr<TestDto> errorResult = Error.NotFound("Test.NotFound", $"Not found: {query.CacheKey}");
            return Task.FromResult(errorResult);
        }

        // Return success
        var dto = new TestDto($"Response for {query.CacheKey}", currentCount);
        ErrorOr<TestDto> successResult = dto;
        return Task.FromResult(successResult);
    }
}
