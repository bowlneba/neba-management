using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Neba.Application.Behaviors;
using Neba.Application.Caching;
using NSubstitute.ReturnsExtensions;

namespace Neba.Application.UnitTests.Behaviors;

public sealed class QueryCachingBehaviorTests
{
    private readonly ICacheService _mockCacheService;
    private readonly IFeatureManager _mockFeatureManager;
    private readonly ILogger<QueryCachingBehavior<TestCacheRequest, TestResponse>> _mockLogger;

    private readonly QueryCachingBehavior<TestCacheRequest, TestResponse> _queryCachingBehavior;

    public QueryCachingBehaviorTests()
    {
        _mockFeatureManager = Substitute.For<IFeatureManager>();
        _mockCacheService = Substitute.For<ICacheService>();
        _mockLogger = Substitute.For<ILogger<QueryCachingBehavior<TestCacheRequest, TestResponse>>>();

        _queryCachingBehavior =
            new QueryCachingBehavior<TestCacheRequest, TestResponse>(_mockFeatureManager, _mockCacheService,
                _mockLogger);
    }

    [Fact]
    public async Task Handle_WhenFeatureIsNotEnabled_ReturnsDatabaseResponse()
    {
        // Arrange
        _mockFeatureManager.IsEnabledAsync(FeatureFlags.Caching).Returns(false);

        var request = new TestCacheRequest();

        _mockCacheService.GetAsync<TestResponse>(request.Key, Arg.Any<CancellationToken>()).ReturnsNull();

        ErrorOr<TestResponse> databaseResponse = new TestResponse { ResponseValue = 2 };

        var cancellationToken = CancellationToken.None;

        // Act
        var response = await _queryCachingBehavior.Handle(request, Next, cancellationToken);

        // Assert
        response.IsError.Should().BeFalse();
        response.Value.ResponseValue.Should().Be(databaseResponse.Value.ResponseValue);

        return;

        Task<ErrorOr<TestResponse>> Next()
        {
            return Task.FromResult(databaseResponse);
        }
    }

    [Fact]
    public async Task Handle_WhenFeatureIsEnabledAndCacheServiceHasAHit_ReturnsCachedResponse()
    {
        // Arrange
        _mockFeatureManager.IsEnabledAsync(FeatureFlags.Caching).Returns(true);

        var request = new TestCacheRequest();

        var cacheResponse = new TestResponse { ResponseValue = 1 };
        var cancellationToken = CancellationToken.None;
        _mockCacheService.GetAsync<TestResponse>(request.Key, cancellationToken)
            .Returns(cacheResponse);

        ErrorOr<TestResponse> databaseResponse = new TestResponse { ResponseValue = 2 };

        // Act
        var response = await _queryCachingBehavior.Handle(request, Next, cancellationToken);

        // Assert
        response.IsError.Should().BeFalse();
        response.Value.ResponseValue.Should().Be(cacheResponse.ResponseValue);

        return;

        Task<ErrorOr<TestResponse>> Next()
        {
            return Task.FromResult(databaseResponse);
        }
    }

    [Fact]
    public async Task Handle_WhenFeatureIsEnabledAndCacheServiceHasAMiss_ResponseHasNoError_ReturnsDatabaseResponse()
    {
        // Arrange
        _mockFeatureManager.IsEnabledAsync(FeatureFlags.Caching).Returns(true);

        var request = new TestCacheRequest();

        _mockCacheService.GetAsync<TestResponse>(request.Key, Arg.Any<CancellationToken>()).ReturnsNull();

        ErrorOr<TestResponse> databaseResponse = new TestResponse { ResponseValue = 2 };

        var cancellationToken = CancellationToken.None;

        // Act
        var response = await _queryCachingBehavior.Handle(request, Next, cancellationToken);

        // Assert
        response.IsError.Should().BeFalse();
        response.Value.ResponseValue.Should().Be(databaseResponse.Value.ResponseValue);

        await _mockCacheService.Received(1)
            .SetAsync(request.Key, databaseResponse.Value, request.Expiration, cancellationToken);

        return;

        Task<ErrorOr<TestResponse>> Next()
        {
            return Task.FromResult(databaseResponse);
        }
    }

    [Fact]
    public async Task Handle_WhenFeatureIsEnabledAndCacheServiceHasAMiss_ResponseHasError_ReturnsErrorResponse()
    {
        // Arrange
        var request = new TestCacheRequest();

        _mockCacheService.GetAsync<TestResponse>(request.Key, Arg.Any<CancellationToken>()).ReturnsNull();

        ErrorOr<TestResponse> databaseResponse = Error.Failure();

        var cancellationToken = CancellationToken.None;

        // Act
        var response = await _queryCachingBehavior.Handle(request, Next, cancellationToken);

        // Assert
        response.IsError.Should().BeTrue();
        response.Errors.Should().BeEquivalentTo(databaseResponse.Errors);

        await _mockCacheService.DidNotReceive()
            .SetAsync(request.Key, databaseResponse, request.Expiration, cancellationToken);

        return;

        Task<ErrorOr<TestResponse>> Next()
        {
            return Task.FromResult(databaseResponse);
        }
    }
}