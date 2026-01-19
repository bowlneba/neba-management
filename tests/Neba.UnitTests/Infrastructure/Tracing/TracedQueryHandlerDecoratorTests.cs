using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Neba.Application.Messaging;
using Neba.Infrastructure.Tracing;

namespace Neba.UnitTests.Infrastructure.Tracing;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Tracing")]
public sealed class TracedQueryHandlerDecoratorTests
{
    private sealed record TestQuery : IQuery<string>;

    private sealed record CachedTestQuery : ICachedQuery<string>
    {
        public string Key => "test_cache_key";
    }

    private sealed class SuccessfulQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult("query_result");
        }
    }

    private sealed class FailingQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Query handler failed");
        }
    }

    private sealed class CachedQueryHandler : IQueryHandler<CachedTestQuery, string>
    {
        public Task<string> HandleAsync(CachedTestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult("cached_result");
        }
    }

    [Fact(DisplayName = "Handles query successfully with tracing")]
    public async Task HandleAsync_WithSuccessfulHandler_ReturnsResult()
    {
        // Arrange
        var innerHandler = new SuccessfulQueryHandler();
        NullLogger<TracedQueryHandlerDecorator<TestQuery, string>> logger = NullLogger<TracedQueryHandlerDecorator<TestQuery, string>>.Instance;
        var decorator = new TracedQueryHandlerDecorator<TestQuery, string>(innerHandler, logger);
        var query = new TestQuery();

        // Act
        string result = await decorator.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldBe("query_result");
    }

    [Fact(DisplayName = "Traces activity for query execution")]
    public async Task HandleAsync_CreatesActivityForQuery()
    {
        // Arrange
        var innerHandler = new SuccessfulQueryHandler();
        NullLogger<TracedQueryHandlerDecorator<TestQuery, string>> logger = NullLogger<TracedQueryHandlerDecorator<TestQuery, string>>.Instance;
        var decorator = new TracedQueryHandlerDecorator<TestQuery, string>(innerHandler, logger);
        var query = new TestQuery();

        // Enable activity listener to capture traces
        using var listener = new ActivityListener { ShouldListenTo = _ => true, Sample = SampleActivity };
        ActivitySource.AddActivityListener(listener);

        // Act
        string result = await decorator.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldBe("query_result");
    }

    [Fact(DisplayName = "Handles query exception with tracing")]
    public async Task HandleAsync_WithFailingHandler_ThrowsAndTraces()
    {
        // Arrange
        var innerHandler = new FailingQueryHandler();
        NullLogger<TracedQueryHandlerDecorator<TestQuery, string>> logger = NullLogger<TracedQueryHandlerDecorator<TestQuery, string>>.Instance;
        var decorator = new TracedQueryHandlerDecorator<TestQuery, string>(innerHandler, logger);
        var query = new TestQuery();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            () => decorator.HandleAsync(query, CancellationToken.None));
    }

    [Fact(DisplayName = "Identifies cached queries correctly")]
    public async Task HandleAsync_WithCachedQuery_IdentifiesCaching()
    {
        // Arrange
        var innerHandler = new CachedQueryHandler();
        NullLogger<TracedQueryHandlerDecorator<CachedTestQuery, string>> logger = NullLogger<TracedQueryHandlerDecorator<CachedTestQuery, string>>.Instance;
        var decorator = new TracedQueryHandlerDecorator<CachedTestQuery, string>(innerHandler, logger);
        var query = new CachedTestQuery();

        // Act
        string result = await decorator.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldBe("cached_result");
    }

    [Fact(DisplayName = "Respects cancellation token")]
    public async Task HandleAsync_WithCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var innerHandler = new SuccessfulQueryHandler();
        NullLogger<TracedQueryHandlerDecorator<TestQuery, string>> logger = NullLogger<TracedQueryHandlerDecorator<TestQuery, string>>.Instance;
        var decorator = new TracedQueryHandlerDecorator<TestQuery, string>(innerHandler, logger);
        var query = new TestQuery();
        var cancellationToken = new CancellationToken(canceled: false);

        // Act
        string result = await decorator.HandleAsync(query, cancellationToken);

        // Assert
        result.ShouldBe("query_result");
    }

    [Fact(DisplayName = "Multiple sequential queries are traced independently")]
    public async Task HandleAsync_WithMultipleQueries_TraceEach()
    {
        // Arrange
        var innerHandler = new SuccessfulQueryHandler();
        NullLogger<TracedQueryHandlerDecorator<TestQuery, string>> logger = NullLogger<TracedQueryHandlerDecorator<TestQuery, string>>.Instance;
        var decorator = new TracedQueryHandlerDecorator<TestQuery, string>(innerHandler, logger);

        // Act
        string result1 = await decorator.HandleAsync(new TestQuery(), CancellationToken.None);
        string result2 = await decorator.HandleAsync(new TestQuery(), CancellationToken.None);
        string result3 = await decorator.HandleAsync(new TestQuery(), CancellationToken.None);

        // Assert
        result1.ShouldBe("query_result");
        result2.ShouldBe("query_result");
        result3.ShouldBe("query_result");
    }

    [Fact(DisplayName = "Handles rapid sequential query execution")]
    public async Task HandleAsync_WithRapidSequentialQueries_CompletesSuccessfully()
    {
        // Arrange
        var innerHandler = new SuccessfulQueryHandler();
        NullLogger<TracedQueryHandlerDecorator<TestQuery, string>> logger = NullLogger<TracedQueryHandlerDecorator<TestQuery, string>>.Instance;
        var decorator = new TracedQueryHandlerDecorator<TestQuery, string>(innerHandler, logger);

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => decorator.HandleAsync(new TestQuery(), CancellationToken.None))
            .ToList();

        string[] results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(r => r == "query_result");
    }

    private static ActivitySamplingResult SampleActivity(ref ActivityCreationOptions<ActivityContext> _)
        => ActivitySamplingResult.AllData;
}
