using System.Net;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Neba.Contracts;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.Documents;
using Neba.Web.Server.History.Awards;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Neba.Website.Contracts.Awards;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.Titles;
using Refit;

namespace Neba.UnitTests.Ui.Server.Services;

public sealed class NebaApiServiceTests
{
    private readonly Mock<INebaApi> _mockNebaApi;
    private readonly NebaApiService _sut;

    public NebaApiServiceTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        _sut = new NebaApiService(_mockNebaApi.Object);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_SuccessfulResponse_ReturnsGroupedAwards()
    {
        // Arrange - Create awards for multiple seasons with multiple categories
        var awards = new List<BowlerOfTheYearResponse>
        {
            BowlerOfTheYearResponseFactory.Create("John Doe", "2024", "Open"),
            BowlerOfTheYearResponseFactory.Create("Jane Smith", "2024", "Woman"),
            BowlerOfTheYearResponseFactory.Create("Bob Johnson", "2023", "Open"),
            BowlerOfTheYearResponseFactory.Create("Alice Brown", "2023", "Senior")
        };

        var collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = awards,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2); // Two seasons

        var awardsList = result.Value.ToList();

        // Verify 2024 awards (should be first due to OrderByDescending)
        awardsList[0].Season.ShouldBe("2024");
        awardsList[0].WinnersByCategory.Count().ShouldBe(2);
        awardsList[0].WinnersByCategory.ShouldContain(kvp => kvp.Key == "Open" && kvp.Value == "John Doe");
        awardsList[0].WinnersByCategory.ShouldContain(kvp => kvp.Key == "Woman" && kvp.Value == "Jane Smith");

        // Verify 2023 awards
        awardsList[1].Season.ShouldBe("2023");
        awardsList[1].WinnersByCategory.Count().ShouldBe(2);
        awardsList[1].WinnersByCategory.ShouldContain(kvp => kvp.Key == "Open" && kvp.Value == "Bob Johnson");
        awardsList[1].WinnersByCategory.ShouldContain(kvp => kvp.Key == "Senior" && kvp.Value == "Alice Brown");
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = new List<BowlerOfTheYearResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = new List<BowlerOfTheYearResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_NullContent_ReturnsError()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<BowlerOfTheYearResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_ApiException_ReturnsError()
    {
        // Arrange
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "API Error" };
        ApiException apiException = await ApiException.Create(httpRequest, HttpMethod.Get, httpResponse, new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ThrowsAsync(apiException);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_TaskCanceledException_ReturnsTimeoutError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetBowlerOfTheYearAwardsAsync_OrdersBySeasonDescending()
    {
        // Arrange - Create awards in non-sorted order
        var awards = new List<BowlerOfTheYearResponse>
        {
            BowlerOfTheYearResponseFactory.Create("Person A", "2020", "Open"),
            BowlerOfTheYearResponseFactory.Create("Person B", "2025", "Open"),
            BowlerOfTheYearResponseFactory.Create("Person C", "2022", "Open"),
            BowlerOfTheYearResponseFactory.Create("Person D", "2024", "Open")
        };

        var collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = awards,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        var awardsList = result.Value.ToList();
        awardsList[0].Season.ShouldBe("2025");
        awardsList[1].Season.ShouldBe("2024");
        awardsList[2].Season.ShouldBe("2022");
        awardsList[3].Season.ShouldBe("2020");
    }

    #region GetTitlesSummaryAsync Tests

    [Fact]
    public async Task GetTitlesSummaryAsync_SuccessfulResponse_ReturnsTitleSummaries()
    {
        // Arrange
        var summaries = new List<TitleSummaryResponse>
        {
            TitleSummaryResponseFactory.Create(bowlerName: "John Doe", titleCount: 5),
            TitleSummaryResponseFactory.Create(bowlerName: "Jane Smith", titleCount: 3)
        };

        var collectionResponse = new CollectionResponse<TitleSummaryResponse>
        {
            Items = summaries,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleSummaryViewModel>> result = await _sut.GetTitlesSummaryAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(x => x.BowlerName == "John Doe");
        result.Value.ShouldContain(x => x.BowlerName == "Jane Smith");
    }

    [Fact]
    public async Task GetTitlesSummaryAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleSummaryResponse>
        {
            Items = new List<TitleSummaryResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleSummaryViewModel>> result = await _sut.GetTitlesSummaryAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetTitlesSummaryAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleSummaryResponse>
        {
            Items = new List<TitleSummaryResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleSummaryViewModel>> result = await _sut.GetTitlesSummaryAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact]
    public async Task GetTitlesSummaryAsync_NullContent_ReturnsError()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<TitleSummaryResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleSummaryViewModel>> result = await _sut.GetTitlesSummaryAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    #endregion

    #region GetBowlerTitlesAsync Tests

    [Fact]
    public async Task GetBowlerTitlesAsync_SuccessfulResponse_ReturnsBowlerTitles()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var bowlerTitles = BowlerTitlesResponseFactory.Create(bowlerId: bowlerId, bowlerName: "John Doe", titleCount: 5);

        var apiResponseData = new Neba.Contracts.ApiResponse<BowlerTitlesResponse>
        {
            Data = bowlerTitles
        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(apiResponseData);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<BowlerTitlesViewModel> result = await _sut.GetBowlerTitlesAsync(bowlerId);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.BowlerName.ShouldBe("John Doe");
        // BowlerTitlesViewModel does not have BowlerId property
    }

    [Fact]
    public async Task GetBowlerTitlesAsync_ApiError_ReturnsError()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var apiResponseData = new Neba.Contracts.ApiResponse<BowlerTitlesResponse>
        {
            Data = BowlerTitlesResponseFactory.Create(titleCount: 1)
        };

        using var apiResponse = ApiResponseFactory.CreateResponse(apiResponseData, HttpStatusCode.NotFound);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<BowlerTitlesViewModel> result = await _sut.GetBowlerTitlesAsync(bowlerId);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact]
    public async Task GetBowlerTitlesAsync_NullContent_ReturnsError()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<Neba.Contracts.ApiResponse<BowlerTitlesResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<BowlerTitlesViewModel> result = await _sut.GetBowlerTitlesAsync(bowlerId);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact]
    public async Task GetBowlerTitlesAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<BowlerTitlesViewModel> result = await _sut.GetBowlerTitlesAsync(bowlerId);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    #endregion

    #region GetHighBlockAwardsAsync Tests

    [Fact]
    public async Task GetHighBlockAwardsAsync_SuccessfulResponse_ReturnsGroupedAwards()
    {
        // Arrange - Create awards for multiple seasons with different scores
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2023", score: 1180),
            HighBlockAwardResponseFactory.Create(bowlerName: "Bob Johnson", season: "2022", score: 1150)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(3); // Three seasons

        var awardsList = result.Value.ToList();

        // Verify 2024 award (should be first due to OrderByDescending)
        awardsList[0].Season.ShouldBe("2024");
        awardsList[0].Score.ShouldBe(1200);
        awardsList[0].Bowlers.Count().ShouldBe(1);
        awardsList[0].Bowlers.First().ShouldBe("John Doe");

        // Verify 2023 award
        awardsList[1].Season.ShouldBe("2023");
        awardsList[1].Score.ShouldBe(1180);
        awardsList[1].Bowlers.First().ShouldBe("Jane Smith");

        // Verify 2022 award
        awardsList[2].Season.ShouldBe("2022");
        awardsList[2].Score.ShouldBe(1150);
        awardsList[2].Bowlers.First().ShouldBe("Bob Johnson");
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_TiedScores_GroupsBowlersTogether()
    {
        // Arrange - Create awards with tied scores in same season
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Bob Johnson", season: "2023", score: 1180)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2); // Two seasons

        var awardsList = result.Value.ToList();

        // Verify 2024 award has two bowlers (tie)
        awardsList[0].Season.ShouldBe("2024");
        awardsList[0].Score.ShouldBe(1200);
        awardsList[0].Bowlers.Count().ShouldBe(2);
        awardsList[0].Bowlers.ShouldContain("John Doe");
        awardsList[0].Bowlers.ShouldContain("Jane Smith");

        // Verify bowlers are ordered alphabetically
        var bowlersList = awardsList[0].Bowlers.ToList();
        bowlersList[0].ShouldBe("Jane Smith");
        bowlersList[1].ShouldBe("John Doe");
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = new List<HighBlockAwardResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = new List<HighBlockAwardResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_NullContent_ReturnsError()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<HighBlockAwardResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_ApiException_ReturnsError()
    {
        // Arrange
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "API Error" };
        ApiException apiException = await ApiException.Create(httpRequest, HttpMethod.Get, httpResponse, new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ThrowsAsync(apiException);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_TaskCanceledException_ReturnsTimeoutError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighBlockAwardsAsync_OrdersBySeasonDescending()
    {
        // Arrange - Create awards in non-sorted order
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "Person A", season: "2020", score: 1100),
            HighBlockAwardResponseFactory.Create(bowlerName: "Person B", season: "2025", score: 1250),
            HighBlockAwardResponseFactory.Create(bowlerName: "Person C", season: "2022", score: 1175),
            HighBlockAwardResponseFactory.Create(bowlerName: "Person D", season: "2024", score: 1200)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        var awardsList = result.Value.ToList();
        awardsList[0].Season.ShouldBe("2025");
        awardsList[1].Season.ShouldBe("2024");
        awardsList[2].Season.ShouldBe("2022");
        awardsList[3].Season.ShouldBe("2020");
    }

    #endregion

    #region GetHighAverageAwardsAsync Tests

    [Fact]
    public async Task GetHighAverageAwardsAsync_SuccessfulResponse_ReturnsAwardsOrderedBySeasonDescending()
    {
        // Arrange
        var awards = new List<HighAverageAwardResponse>
        {
            HighAverageAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", average: 215.50m, games: 42, tournaments: 9),
            HighAverageAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2023", average: 210.75m, games: 40, tournaments: 8),
            HighAverageAwardResponseFactory.Create(bowlerName: "Bob Johnson", season: "2022", average: 208.25m, games: 38, tournaments: 7)
        };

        var collectionResponse = new CollectionResponse<HighAverageAwardResponse>
        {
            Items = awards,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(3);

        var awardsList = result.Value.ToList();

        // Verify ordering by season descending
        awardsList[0].Season.ShouldBe("2024");
        awardsList[0].BowlerName.ShouldBe("John Doe");
        awardsList[0].Average.ShouldBe(215.50m);
        awardsList[0].GamesBowled.ShouldBe(42);
        awardsList[0].TournamentsBowled.ShouldBe(9);

        awardsList[1].Season.ShouldBe("2023");
        awardsList[1].BowlerName.ShouldBe("Jane Smith");

        awardsList[2].Season.ShouldBe("2022");
        awardsList[2].BowlerName.ShouldBe("Bob Johnson");
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighAverageAwardResponse>
        {
            Items = new List<HighAverageAwardResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighAverageAwardResponse>
        {
            Items = new List<HighAverageAwardResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_NullContent_ReturnsError()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<HighAverageAwardResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_ApiException_ReturnsError()
    {
        // Arrange
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "API Error" };
        ApiException apiException = await ApiException.Create(httpRequest, HttpMethod.Get, httpResponse, new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ThrowsAsync(apiException);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_TaskCanceledException_ReturnsTimeoutError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetHighAverageAwardsAsync_OrdersBySeasonDescending()
    {
        // Arrange - Create awards in non-sorted order
        var awards = new List<HighAverageAwardResponse>
        {
            HighAverageAwardResponseFactory.Create(bowlerName: "Person A", season: "2020", average: 200.5m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Person B", season: "2025", average: 220.0m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Person C", season: "2022", average: 210.25m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Person D", season: "2024", average: 215.75m)
        };

        var collectionResponse = new CollectionResponse<HighAverageAwardResponse>
        {
            Items = awards,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        var awardsList = result.Value.ToList();
        awardsList[0].Season.ShouldBe("2025");
        awardsList[1].Season.ShouldBe("2024");
        awardsList[2].Season.ShouldBe("2022");
        awardsList[3].Season.ShouldBe("2020");
    }

    #endregion

    #region GetTournamentRulesAsync Tests

    [Fact]
    public async Task GetTournamentRulesAsync_SuccessfulResponse_ReturnsMarkupString()
    {
        // Arrange
        const string htmlContent = "<h1>Tournament Rules</h1><p>These are the rules.</p>";

        using var apiResponse = ApiResponseFactory.CreateDocumentResponse(htmlContent);

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Content.Value.ShouldBe(htmlContent);
    }

    [Fact]
    public async Task GetTournamentRulesAsync_EmptyContent_ReturnsEmptyMarkupString()
    {
        // Arrange
        const string htmlContent = "";

        using var apiResponse = ApiResponseFactory.CreateDocumentResponse(htmlContent);

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Content.Value.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task GetTournamentRulesAsync_ApiError_ReturnsError()
    {
        // Arrange
        using var apiResponse = ApiResponseFactory.CreateDocumentResponse("", HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }
    [Fact]
    public async Task GetTournamentRulesAsync_ApiException_ReturnsError()
    {
        // Arrange
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "API Error" };
        ApiException apiException = await ApiException.Create(httpRequest, HttpMethod.Get, httpResponse, new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ThrowsAsync(apiException);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetTournamentRulesAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetTournamentRulesAsync_TaskCanceledException_ReturnsTimeoutError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetTournamentRulesAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetTournamentRulesAsync_ComplexHtmlContent_ReturnsCorrectMarkupString()
    {
        // Arrange
        const string htmlContent = @"
            <div class='rules-container'>
                <h1>NEBA Tournament Rules</h1>
                <ol>
                    <li><strong>Registration:</strong> All participants must register before the tournament.</li>
                    <li><strong>Equipment:</strong> Only approved equipment is allowed.</li>
                    <li><strong>Scoring:</strong> Standard bowling scoring rules apply.</li>
                </ol>
                <p>For more information, contact the tournament director.</p>
            </div>";

        using var apiResponse = ApiResponseFactory.CreateDocumentResponse(htmlContent);

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Content.Value.ShouldBe(htmlContent);
    }

    #endregion

    #region GetTitlesByYearAsync Tests

    [Fact]
    public async Task GetTitlesByYearAsync_SuccessfulResponse_ReturnsTitlesGroupedByYear()
    {
        // Arrange
        var titles = new List<TitleResponse>
        {
            TitleResponseFactory.Create(bowlerName: "John Doe", tournamentYear: 2024, tournamentMonth: Month.January),
            TitleResponseFactory.Create(bowlerName: "Jane Smith", tournamentYear: 2024, tournamentMonth: Month.March),
            TitleResponseFactory.Create(bowlerName: "Bob Johnson", tournamentYear: 2023, tournamentMonth: Month.February)
        };

        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = titles,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>> result = await _sut.GetTitlesByYearAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2); // Two years: 2024 and 2023

        var titlesList = result.Value.ToList();

        // Verify years are ordered descending
        titlesList[0].Year.ShouldBe(2024);
        titlesList[1].Year.ShouldBe(2023);

        // Verify 2024 has 2 titles
        titlesList[0].Titles.Count.ShouldBe(2);

        // Verify 2023 has 1 title
        titlesList[1].Titles.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetTitlesByYearAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = new List<TitleResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>> result = await _sut.GetTitlesByYearAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetTitlesByYearAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = new List<TitleResponse>(),

        };

        using var apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>> result = await _sut.GetTitlesByYearAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact]
    public async Task GetTitlesByYearAsync_NullContent_ReturnsError()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<TitleResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>> result = await _sut.GetTitlesByYearAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact]
    public async Task GetTitlesByYearAsync_OrdersByYearDescendingAndMonthDescending()
    {
        // Arrange
        var titles = new List<TitleResponse>
        {
            TitleResponseFactory.Create(bowlerName: "Person A", tournamentYear: 2020, tournamentMonth: Month.January),
            TitleResponseFactory.Create(bowlerName: "Person B", tournamentYear: 2025, tournamentMonth: Month.December),
            TitleResponseFactory.Create(bowlerName: "Person C", tournamentYear: 2025, tournamentMonth: Month.January),
            TitleResponseFactory.Create(bowlerName: "Person D", tournamentYear: 2024, tournamentMonth: Month.June)
        };

        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = titles,

        };

        using var apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>> result = await _sut.GetTitlesByYearAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        var titlesList = result.Value.ToList();

        // Verify years are in descending order
        titlesList[0].Year.ShouldBe(2025);
        titlesList[1].Year.ShouldBe(2024);
        titlesList[2].Year.ShouldBe(2020);

        // Verify titles within 2025 are ordered by month descending
        var titles2025 = titlesList[0].Titles.ToList();
        titles2025[0].BowlerName.ShouldBe("Person B"); // December
        titles2025[1].BowlerName.ShouldBe("Person C"); // January
    }

    #endregion
}
