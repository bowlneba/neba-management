#pragma warning disable CA2000 // Dispose objects before losing scope - Mock objects in tests don't need disposal

using Moq;
using Neba.Contracts;
using Neba.Contracts.History.Champions;
using Neba.Web.Server;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Refit;
using System.Net;

namespace Neba.UnitTests.Services;

public sealed class NebaApiServiceTests
{
    private readonly Mock<INebaApi> _mockNebaApi;
    private readonly NebaApiService _service;

    public NebaApiServiceTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        _service = new NebaApiService(_mockNebaApi.Object);
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenSuccessful_ReturnsSuccessResult()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var bowlers = new List<GetBowlerTitleCountsResponse>
        {
            new() { BowlerId = bowlerId, BowlerName = "John Doe", TitleCount = 5 }
        };

        var expectedResponse = new Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new CollectionResponse<GetBowlerTitleCountsResponse>
            {
                Items = bowlers
            },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(false);
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(1);
        var viewModel = result.Value.First();
        viewModel.BowlerId.ShouldBe(bowlerId);
        viewModel.BowlerName.ShouldBe("John Doe");
        viewModel.Titles.ShouldBe(5);
        _mockNebaApi.Verify(x => x.GetBowlerTitleCountsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WithMultipleBowlers_ReturnsAllBowlers()
    {
        // Arrange
        var bowler1 = new GetBowlerTitleCountsResponse
        {
            BowlerId = Guid.NewGuid(),
            BowlerName = "John Doe",
            TitleCount = 5
        };

        var bowler2 = new GetBowlerTitleCountsResponse
        {
            BowlerId = Guid.NewGuid(),
            BowlerName = "Jane Smith",
            TitleCount = 3
        };

        var expectedResponse = new Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new CollectionResponse<GetBowlerTitleCountsResponse>
            {
                Items = new List<GetBowlerTitleCountsResponse> { bowler1, bowler2 }
            },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(false);
        result.Value.Count.ShouldBe(2);

        var firstBowler = result.Value.First();
        firstBowler.BowlerId.ShouldBe(bowler1.BowlerId);
        firstBowler.BowlerName.ShouldBe("John Doe");
        firstBowler.Titles.ShouldBe(5);

        var secondBowler = result.Value.Last();
        secondBowler.BowlerId.ShouldBe(bowler2.BowlerId);
        secondBowler.BowlerName.ShouldBe("Jane Smith");
        secondBowler.Titles.ShouldBe(3);
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenApiFails_ReturnsError()
    {
        // Arrange
        var expectedResponse = new Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Internal Server Error"
            },
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(true);
        result.FirstError.Code.ShouldBe("Api.RequestFailed");
        result.FirstError.Description.ShouldContain("Internal Server Error");
        _mockNebaApi.Verify(x => x.GetBowlerTitleCountsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenApiReturnsNotFound_ReturnsError()
    {
        // Arrange
        var expectedResponse = new Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>(
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                ReasonPhrase = "Not Found"
            },
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(true);
        result.FirstError.Code.ShouldBe("Api.RequestFailed");
        result.FirstError.Description.ShouldContain("Not Found");
        _mockNebaApi.Verify(x => x.GetBowlerTitleCountsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenApiReturnsNullContent_ReturnsError()
    {
        // Arrange
        var expectedResponse = new Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(true);
        result.FirstError.Code.ShouldBe("Api.RequestFailed");
        result.FirstError.Description.ShouldContain("No content returned");
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenApiReturnsEmptyList_ReturnsEmptyCollection()
    {
        // Arrange
        var expectedResponse = new Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new CollectionResponse<GetBowlerTitleCountsResponse>
            {
                Items = new List<GetBowlerTitleCountsResponse>()
            },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(false);
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenApiExceptionThrown_ReturnsError()
    {
        // Arrange
        var apiException = await Refit.ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Bad Request"
            },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ThrowsAsync(apiException);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(true);
        result.FirstError.Code.ShouldBe("Api.RequestFailed");
        result.FirstError.Description.ShouldContain("Bad Request");
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenHttpRequestExceptionThrown_ReturnsNetworkError()
    {
        // Arrange
        var httpException = new HttpRequestException("Network error occurred");

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ThrowsAsync(httpException);

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(true);
        result.FirstError.Code.ShouldBe("Api.NetworkError");
        result.FirstError.Description.ShouldContain("Network error occurred");
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenTaskCanceledExceptionThrown_ReturnsTimeoutError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ThrowsAsync(new TaskCanceledException());

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(true);
        result.FirstError.Code.ShouldBe("Api.Timeout");
        result.FirstError.Description.ShouldBe("Request timeout");
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_WhenUnexpectedExceptionThrown_ReturnsUnexpectedError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        // Act
        var result = await _service.GetBowlerTitleCountsAsync();

        // Assert
        result.IsError.ShouldBe(true);
        result.FirstError.Code.ShouldBe("Api.Unexpected");
        result.FirstError.Description.ShouldContain("Something went wrong");
    }

    [Fact]
    public async Task GetBowlerTitleCountsAsync_CallsApiOnlyOnce()
    {
        // Arrange
        var expectedResponse = new Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new CollectionResponse<GetBowlerTitleCountsResponse>
            {
                Items = new List<GetBowlerTitleCountsResponse>()
            },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        await _service.GetBowlerTitleCountsAsync();

        // Assert
        _mockNebaApi.Verify(x => x.GetBowlerTitleCountsAsync(), Times.Once);
        _mockNebaApi.VerifyNoOtherCalls();
    }
}
