using System.Net;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Neba.Contracts;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.BowlingCenters;
using Neba.Web.Server.Documents;
using Neba.Web.Server.History.Awards;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Neba.Web.Server.Tournaments;
using Neba.Website.Contracts.Awards;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.BowlingCenters;
using Neba.Website.Contracts.Titles;
using Neba.Website.Contracts.Tournaments;
using Refit;

namespace Neba.UnitTests.Ui.Server.Services;

[Trait("Category", "Unit")]
[Trait("Component", "Ui.Server.Services")]

public sealed class NebaWebsiteApiServiceTests
{
    private readonly Mock<INebaWebsiteApi> _mockNebaApi;
    private readonly NebaWebsiteApiService _sut;

    public NebaWebsiteApiServiceTests()
    {
        _mockNebaApi = new Mock<INebaWebsiteApi>();
        _sut = new NebaWebsiteApiService(_mockNebaApi.Object);
    }

    [Fact(DisplayName = "Returns grouped Bowler of the Year awards on successful response")]
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
            Items = awards

        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns empty collection when API returns empty response")]
    public async Task GetBowlerOfTheYearAwardsAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = new List<BowlerOfTheYearResponse>()

        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when API call fails")]
    public async Task GetBowlerOfTheYearAwardsAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = new List<BowlerOfTheYearResponse>()

        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>> result = await _sut.GetBowlerOfTheYearAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns error when API response content is null")]
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

    [Fact(DisplayName = "Returns error when API exception is thrown")]
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

    [Fact(DisplayName = "Returns network error when HTTP request exception is thrown")]
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

    [Fact(DisplayName = "Returns timeout error when task is canceled")]
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

    [Fact(DisplayName = "Returns unexpected error for unhandled exceptions")]
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

    [Fact(DisplayName = "Orders Bowler of the Year awards by season in descending order")]
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
            Items = awards

        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns title summaries on successful response")]
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
            Items = summaries

        };

        using TestApiResponse<CollectionResponse<TitleSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns empty collection when no title summaries exist")]
    public async Task GetTitlesSummaryAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleSummaryResponse>
        {
            Items = new List<TitleSummaryResponse>()

        };

        using TestApiResponse<CollectionResponse<TitleSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleSummaryViewModel>> result = await _sut.GetTitlesSummaryAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when titles summary API call fails")]
    public async Task GetTitlesSummaryAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleSummaryResponse>
        {
            Items = new List<TitleSummaryResponse>()

        };

        using TestApiResponse<CollectionResponse<TitleSummaryResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleSummaryViewModel>> result = await _sut.GetTitlesSummaryAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns error when titles summary response content is null")]
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

    [Fact(DisplayName = "Returns bowler titles on successful response")]
    public async Task GetBowlerTitlesAsync_SuccessfulResponse_ReturnsBowlerTitles()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        BowlerTitlesResponse bowlerTitles = BowlerTitlesResponseFactory.Create(bowlerId: bowlerId, bowlerName: "John Doe", titleCount: 5);

        var apiResponseData = new Neba.Contracts.ApiResponse<BowlerTitlesResponse>
        {
            Data = bowlerTitles
        };

        using TestApiResponse<Contracts.ApiResponse<BowlerTitlesResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(apiResponseData);

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

    [Fact(DisplayName = "Returns error when bowler titles API call fails")]
    public async Task GetBowlerTitlesAsync_ApiError_ReturnsError()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var apiResponseData = new Neba.Contracts.ApiResponse<BowlerTitlesResponse>
        {
            Data = BowlerTitlesResponseFactory.Create(titleCount: 1)
        };

        using TestApiResponse<Contracts.ApiResponse<BowlerTitlesResponse>> apiResponse = ApiResponseFactory.CreateResponse(apiResponseData, HttpStatusCode.NotFound);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<BowlerTitlesViewModel> result = await _sut.GetBowlerTitlesAsync(bowlerId);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns error when bowler titles response content is null")]
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

    [Fact(DisplayName = "Returns network error when bowler titles request fails")]
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

    [Fact(DisplayName = "Returns grouped high block awards on successful response")]
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
            Items = awards

        };

        using TestApiResponse<CollectionResponse<HighBlockAwardResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Groups bowlers together when scores are tied")]
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
            Items = awards

        };

        using TestApiResponse<CollectionResponse<HighBlockAwardResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns empty collection when no high block awards exist")]
    public async Task GetHighBlockAwardsAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = new List<HighBlockAwardResponse>()

        };

        using TestApiResponse<CollectionResponse<HighBlockAwardResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when high block awards API call fails")]
    public async Task GetHighBlockAwardsAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = new List<HighBlockAwardResponse>()

        };

        using TestApiResponse<CollectionResponse<HighBlockAwardResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>> result = await _sut.GetHighBlockAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns error when high block awards response content is null")]
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

    [Fact(DisplayName = "Returns error when high block awards API exception is thrown")]
    public async Task GetHighBlockAwardsAsync_ApiException_ReturnsError()
    {
        // Arrange
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        httpResponse.ReasonPhrase = "API Error";
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

    [Fact(DisplayName = "Returns network error when high block awards request fails")]
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

    [Fact(DisplayName = "Returns timeout error when high block awards task is canceled")]
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

    [Fact(DisplayName = "Returns unexpected error for unhandled high block awards exceptions")]
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

    [Fact(DisplayName = "Orders high block awards by season in descending order")]
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
            Items = awards

        };

        using TestApiResponse<CollectionResponse<HighBlockAwardResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns high average awards ordered by season descending")]
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
            Items = awards

        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns empty collection when no high average awards exist")]
    public async Task GetHighAverageAwardsAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighAverageAwardResponse>
        {
            Items = new List<HighAverageAwardResponse>()

        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when high average awards API call fails")]
    public async Task GetHighAverageAwardsAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighAverageAwardResponse>
        {
            Items = new List<HighAverageAwardResponse>()

        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>> result = await _sut.GetHighAverageAwardsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns error when high average awards response content is null")]
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

    [Fact(DisplayName = "Returns error when high average awards API exception is thrown")]
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

    [Fact(DisplayName = "Returns network error when high average awards request fails")]
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

    [Fact(DisplayName = "Returns timeout error when high average awards task is canceled")]
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

    [Fact(DisplayName = "Returns unexpected error for unhandled high average awards exceptions")]
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

    [Fact(DisplayName = "Orders high average awards by season in descending order")]
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
            Items = awards

        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns tournament rules as markup string on successful response")]
    public async Task GetTournamentRulesAsync_SuccessfulResponse_ReturnsMarkupString()
    {
        // Arrange
        const string htmlContent = "<h1>Tournament Rules</h1><p>These are the rules.</p>";

        using TestApiResponse<DocumentResponse<string>> apiResponse = ApiResponseFactory.CreateDocumentResponse(htmlContent);

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Content.Value.ShouldBe(htmlContent);
    }

    [Fact(DisplayName = "Returns empty markup string when tournament rules content is empty")]
    public async Task GetTournamentRulesAsync_EmptyContent_ReturnsEmptyMarkupString()
    {
        // Arrange
        const string htmlContent = "";

        using TestApiResponse<DocumentResponse<string>> apiResponse = ApiResponseFactory.CreateDocumentResponse(htmlContent);

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Content.Value.ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "Returns error when tournament rules API call fails")]
    public async Task GetTournamentRulesAsync_ApiError_ReturnsError()
    {
        // Arrange
        using TestApiResponse<DocumentResponse<string>> apiResponse = ApiResponseFactory.CreateDocumentResponse("", HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<DocumentViewModel<MarkupString>> result = await _sut.GetTournamentRulesAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }
    [Fact(DisplayName = "Returns error when tournament rules API exception is thrown")]
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

    [Fact(DisplayName = "Returns network error when tournament rules request fails")]
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

    [Fact(DisplayName = "Returns timeout error when tournament rules task is canceled")]
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

    [Fact(DisplayName = "Returns unexpected error for unhandled tournament rules exceptions")]
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

    [Fact(DisplayName = "Returns correct markup string for complex HTML content")]
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

        using TestApiResponse<DocumentResponse<string>> apiResponse = ApiResponseFactory.CreateDocumentResponse(htmlContent);

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

    [Fact(DisplayName = "Returns titles grouped by year on successful response")]
    public async Task GetTitlesByYearAsync_SuccessfulResponse_ReturnsTitlesGroupedByYear()
    {
        // Arrange
        var titles = new List<TitleResponse>
        {
            TitleResponseFactory.Create(bowlerName: "John Doe", tournamentMonth: Month.January, tournamentYear: 2024),
            TitleResponseFactory.Create(bowlerName: "Jane Smith", tournamentMonth: Month.March, tournamentYear: 2024),
            TitleResponseFactory.Create(bowlerName: "Bob Johnson", tournamentMonth: Month.February, tournamentYear: 2023)
        };

        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = titles

        };

        using TestApiResponse<CollectionResponse<TitleResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    [Fact(DisplayName = "Returns empty collection when no titles exist")]
    public async Task GetTitlesByYearAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = new List<TitleResponse>()

        };

        using TestApiResponse<CollectionResponse<TitleResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>> result = await _sut.GetTitlesByYearAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when titles by year API call fails")]
    public async Task GetTitlesByYearAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = new List<TitleResponse>()

        };

        using TestApiResponse<CollectionResponse<TitleResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>> result = await _sut.GetTitlesByYearAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns error when titles by year response content is null")]
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

    [Fact(DisplayName = "Orders titles by year and month in descending order")]
    public async Task GetTitlesByYearAsync_OrdersByYearDescendingAndMonthDescending()
    {
        // Arrange
        var titles = new List<TitleResponse>
        {
            TitleResponseFactory.Create(bowlerName: "Person A", tournamentMonth: Month.January, tournamentYear: 2020),
            TitleResponseFactory.Create(bowlerName: "Person B", tournamentMonth: Month.December, tournamentYear: 2025),
            TitleResponseFactory.Create(bowlerName: "Person C", tournamentMonth: Month.January, tournamentYear: 2025),
            TitleResponseFactory.Create(bowlerName: "Person D", tournamentMonth: Month.June, tournamentYear: 2024)
        };

        var collectionResponse = new CollectionResponse<TitleResponse>
        {
            Items = titles

        };

        using TestApiResponse<CollectionResponse<TitleResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

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

    #region GetBowlingCentersAsync Tests

    [Fact(DisplayName = "Returns bowling centers ordered by name on successful response")]
    public async Task GetBowlingCentersAsync_SuccessfulResponse_ReturnsBowlingCentersOrderedByName()
    {
        // Arrange - Create bowling centers with names in non-alphabetical order
        var centers = new List<BowlingCenterResponse>
        {
            BowlingCenterResponseFactory.Create(name: "Zebra Lanes"),
            BowlingCenterResponseFactory.Create(name: "Alpha Bowling"),
            BowlingCenterResponseFactory.Create(name: "Charlie's Bowl")
        };

        var collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers

        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(3);

        var centersList = result.Value.ToList();

        // Verify centers are ordered by name in ascending order
        centersList[0].Name.ShouldBe("Alpha Bowling");
        centersList[1].Name.ShouldBe("Charlie's Bowl");
        centersList[2].Name.ShouldBe("Zebra Lanes");
    }

    [Fact(DisplayName = "Returns empty collection when no bowling centers exist")]
    public async Task GetBowlingCentersAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = new List<BowlingCenterResponse>()

        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when bowling centers API call fails")]
    public async Task GetBowlingCentersAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = new List<BowlingCenterResponse>()

        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns error when bowling centers response content is null")]
    public async Task GetBowlingCentersAsync_NullContent_ReturnsError()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<BowlingCenterResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns error when bowling centers API exception is thrown")]
    public async Task GetBowlingCentersAsync_ApiException_ReturnsError()
    {
        // Arrange
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "API Error" };
        ApiException apiException = await ApiException.Create(httpRequest, HttpMethod.Get, httpResponse, new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ThrowsAsync(apiException);

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns network error when bowling centers request fails")]
    public async Task GetBowlingCentersAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns timeout error when bowling centers task is canceled")]
    public async Task GetBowlingCentersAsync_TaskCanceledException_ReturnsTimeoutError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Returns unexpected error for unhandled bowling centers exceptions")]
    public async Task GetBowlingCentersAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Correctly maps BowlingCenterResponse to BowlingCenterViewModel")]
    public async Task GetBowlingCentersAsync_SuccessfulResponse_CorrectlyMapsToViewModel()
    {
        // Arrange
        BowlingCenterResponse center = BowlingCenterResponseFactory.Create(
            name: "Test Bowling Center",
            street: "123 Test St",
            unit: "Suite 100",
            city: "Test City",
            zipCode: "12345",
            phoneNumber: "16315551234",
            phoneExtension: "123",
            latitude: 40.7128,
            longitude: -74.0060,
            isClosed: true);

        var collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = new List<BowlingCenterResponse> { center }

        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(1);

        BowlingCenterViewModel viewModel = result.Value.First();
        viewModel.Name.ShouldBe("Test Bowling Center");
        viewModel.Street.ShouldBe("123 Test St");
        viewModel.Unit.ShouldBe("Suite 100");
        viewModel.City.ShouldBe("Test City");
        viewModel.ZipCode.ShouldBe("12345");
        viewModel.PhoneNumber.ShouldBe("16315551234");
        viewModel.PhoneExtension.ShouldBe("123");
        viewModel.Latitude.ShouldBe(40.7128);
        viewModel.Longitude.ShouldBe(-74.0060);
        viewModel.IsClosed.ShouldBeTrue();
    }

    [Fact(DisplayName = "Orders bowling centers by name in ascending order")]
    public async Task GetBowlingCentersAsync_OrdersByNameAscending()
    {
        // Arrange - Create centers in non-sorted order
        var centers = new List<BowlingCenterResponse>
        {
            BowlingCenterResponseFactory.Create(name: "Downtown Bowl"),
            BowlingCenterResponseFactory.Create(name: "AMF Bowling"),
            BowlingCenterResponseFactory.Create(name: "Strikes & Spares"),
            BowlingCenterResponseFactory.Create(name: "Bowl-O-Rama")
        };

        var collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers

        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>> result = await _sut.GetBowlingCentersAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        var centersList = result.Value.ToList();
        centersList[0].Name.ShouldBe("AMF Bowling");
        centersList[1].Name.ShouldBe("Bowl-O-Rama");
        centersList[2].Name.ShouldBe("Downtown Bowl");
        centersList[3].Name.ShouldBe("Strikes & Spares");
    }

    #endregion

    #region GetFutureTournamentsAsync Tests

    [Fact(DisplayName = "Returns future tournaments ordered by start date on successful response")]
    public async Task GetFutureTournamentsAsync_SuccessfulResponse_ReturnsTournamentsOrderedByStartDate()
    {
        // Arrange
        var tournaments = new List<TournamentSummaryResponse>
        {
            TournamentSummaryResponseFactory.Create(
                name: "Summer Championship",
                startDate: new DateOnly(2026, 7, 15),
                endDate: new DateOnly(2026, 7, 17),
                bowlingCenterName: "Strike Zone Lanes"),
            TournamentSummaryResponseFactory.Create(
                name: "Spring Open",
                startDate: new DateOnly(2026, 4, 10),
                endDate: new DateOnly(2026, 4, 12),
                bowlingCenterName: "Downtown Bowl"),
            TournamentSummaryResponseFactory.Create(
                name: "Winter Classic",
                startDate: new DateOnly(2026, 12, 5),
                endDate: new DateOnly(2026, 12, 7),
                bowlingCenterName: "Perfect Strike")
        };

        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(3);

        var tournamentsList = result.Value.ToList();
        tournamentsList[0].Name.ShouldBe("Spring Open"); // April comes first
        tournamentsList[1].Name.ShouldBe("Summer Championship"); // July
        tournamentsList[2].Name.ShouldBe("Winter Classic"); // December
    }

    [Fact(DisplayName = "Returns empty collection when no future tournaments exist")]
    public async Task GetFutureTournamentsAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = new List<TournamentSummaryResponse>()
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when future tournaments API call fails")]
    public async Task GetFutureTournamentsAsync_ApiError_ReturnsError()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = new List<TournamentSummaryResponse>()
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns error when future tournaments response content is null")]
    public async Task GetFutureTournamentsAsync_NullContent_ReturnsError()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<TournamentSummaryResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns error when future tournaments API exception is thrown")]
    public async Task GetFutureTournamentsAsync_ApiException_ReturnsError()
    {
        // Arrange
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "API Error" };
        ApiException apiException = await ApiException.Create(httpRequest, HttpMethod.Get, httpResponse, new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ThrowsAsync(apiException);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns network error when future tournaments request fails")]
    public async Task GetFutureTournamentsAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns timeout error when future tournaments task is canceled")]
    public async Task GetFutureTournamentsAsync_TaskCanceledException_ReturnsTimeoutError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns unexpected error for unhandled future tournaments exceptions")]
    public async Task GetFutureTournamentsAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Correctly maps TournamentSummaryResponse to TournamentSummaryViewModel")]
    public async Task GetFutureTournamentsAsync_SuccessfulResponse_CorrectlyMapsToViewModel()
    {
        // Arrange
        var tournamentId = TournamentId.New();
        var bowlingCenterId = BowlingCenterId.New();
        var startDate = new DateOnly(2026, 6, 1);
        var endDate = new DateOnly(2026, 6, 3);
        var thumbnailUrl = new Uri("https://example.com/thumbnail.jpg");

        var tournaments = new List<TournamentSummaryResponse>
        {
            TournamentSummaryResponseFactory.Create(
                id: tournamentId,
                name: "Test Tournament",
                startDate: startDate,
                endDate: endDate,
                bowlingCenterId: bowlingCenterId,
                bowlingCenterName: "Test Center",
                thumbnailUrl: thumbnailUrl,
                tournamentType: TournamentType.Doubles)
        };

        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetFutureTournamentsAsync();

        // Assert
        result.IsError.ShouldBeFalse();
        var tournament = result.Value.First();
        tournament.Id.ShouldBe(tournamentId);
        tournament.Name.ShouldBe("Test Tournament");
        tournament.StartDate.ShouldBe(startDate);
        tournament.EndDate.ShouldBe(endDate);
        tournament.BowlingCenterId.ShouldBe(bowlingCenterId);
        tournament.BowlingCenterName.ShouldBe("Test Center");
        tournament.ThumbnailUrl.ShouldBe(thumbnailUrl);
        tournament.TournamentType.ShouldBe(TournamentType.Doubles.Name);
    }

    #endregion

    #region GetTournamentsInAYearAsync Tests

    [Fact(DisplayName = "Returns tournaments in year ordered by start date on successful response")]
    public async Task GetTournamentsInAYearAsync_SuccessfulResponse_ReturnsTournamentsOrderedByStartDate()
    {
        // Arrange
        const int year = 2025;
        var tournaments = new List<TournamentSummaryResponse>
        {
            TournamentSummaryResponseFactory.Create(
                name: "Fall Classic",
                startDate: new DateOnly(2025, 10, 15),
                endDate: new DateOnly(2025, 10, 17),
                bowlingCenterName: "Strike Zone"),
            TournamentSummaryResponseFactory.Create(
                name: "Spring Open",
                startDate: new DateOnly(2025, 3, 10),
                endDate: new DateOnly(2025, 3, 12),
                bowlingCenterName: "Downtown Bowl"),
            TournamentSummaryResponseFactory.Create(
                name: "Summer Championship",
                startDate: new DateOnly(2025, 7, 5),
                endDate: new DateOnly(2025, 7, 7),
                bowlingCenterName: "Perfect Strike")
        };

        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(3);

        var tournamentsList = result.Value.ToList();
        tournamentsList[0].Name.ShouldBe("Spring Open"); // March
        tournamentsList[1].Name.ShouldBe("Summer Championship"); // July
        tournamentsList[2].Name.ShouldBe("Fall Classic"); // October
    }

    [Fact(DisplayName = "Returns empty collection when no tournaments in year")]
    public async Task GetTournamentsInAYearAsync_EmptyResponse_ReturnsEmptyCollection()
    {
        // Arrange
        const int year = 2020;
        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = new List<TournamentSummaryResponse>()
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns error when tournaments in year API call fails")]
    public async Task GetTournamentsInAYearAsync_ApiError_ReturnsError()
    {
        // Arrange
        const int year = 2024;
        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = new List<TournamentSummaryResponse>()
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateResponse(collectionResponse, HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns error when tournaments in year response content is null")]
    public async Task GetTournamentsInAYearAsync_NullContent_ReturnsError()
    {
        // Arrange
        const int year = 2023;
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        using var apiResponse = new Refit.ApiResponse<CollectionResponse<TournamentSummaryResponse>>(
            httpResponse,
            null,
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ReturnsAsync(apiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns error when tournaments in year API exception is thrown")]
    public async Task GetTournamentsInAYearAsync_ApiException_ReturnsError()
    {
        // Arrange
        const int year = 2024;
        using var httpRequest = new HttpRequestMessage();
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "API Error" };
        ApiException apiException = await ApiException.Create(httpRequest, HttpMethod.Get, httpResponse, new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ThrowsAsync(apiException);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns network error when tournaments in year request fails")]
    public async Task GetTournamentsInAYearAsync_HttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        const int year = 2025;
        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns timeout error when tournaments in year task is canceled")]
    public async Task GetTournamentsInAYearAsync_TaskCanceledException_ReturnsTimeoutError()
    {
        // Arrange
        const int year = 2026;
        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Returns unexpected error for unhandled tournaments in year exceptions")]
    public async Task GetTournamentsInAYearAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        // Arrange
        const int year = 2024;
        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact(DisplayName = "Correctly maps tournaments with different years")]
    public async Task GetTournamentsInAYearAsync_DifferentYears_HandlesCorrectly()
    {
        // Arrange
        const int year2024 = 2024;
        const int year2025 = 2025;

        var tournaments2024 = new List<TournamentSummaryResponse>
        {
            TournamentSummaryResponseFactory.Create(
                name: "2024 Tournament",
                startDate: new DateOnly(2024, 5, 10))
        };

        var tournaments2025 = new List<TournamentSummaryResponse>
        {
            TournamentSummaryResponseFactory.Create(
                name: "2025 Tournament",
                startDate: new DateOnly(2025, 5, 10))
        };

        var collectionResponse2024 = new CollectionResponse<TournamentSummaryResponse> { Items = tournaments2024 };
        var collectionResponse2025 = new CollectionResponse<TournamentSummaryResponse> { Items = tournaments2025 };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse2024 = ApiResponseFactory.CreateSuccessResponse(collectionResponse2024);
        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse2025 = ApiResponseFactory.CreateSuccessResponse(collectionResponse2025);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year2024))
            .ReturnsAsync(apiResponse2024.ApiResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year2025))
            .ReturnsAsync(apiResponse2025.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result2024 = await _sut.GetTournamentsInAYearAsync(year2024);
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result2025 = await _sut.GetTournamentsInAYearAsync(year2025);

        // Assert
        result2024.IsError.ShouldBeFalse();
        result2024.Value.First().Name.ShouldBe("2024 Tournament");

        result2025.IsError.ShouldBeFalse();
        result2025.Value.First().Name.ShouldBe("2025 Tournament");
    }

    [Fact(DisplayName = "Correctly maps TournamentSummaryResponse to TournamentSummaryViewModel for year query")]
    public async Task GetTournamentsInAYearAsync_SuccessfulResponse_CorrectlyMapsToViewModel()
    {
        // Arrange
        const int year = 2025;
        var tournamentId = TournamentId.New();
        var bowlingCenterId = BowlingCenterId.New();
        var startDate = new DateOnly(2025, 8, 15);
        var endDate = new DateOnly(2025, 8, 17);
        var thumbnailUrl = new Uri("https://example.com/tournament-thumb.jpg");

        var tournaments = new List<TournamentSummaryResponse>
        {
            TournamentSummaryResponseFactory.Create(
                id: tournamentId,
                name: "2025 Championship",
                startDate: startDate,
                endDate: endDate,
                bowlingCenterId: bowlingCenterId,
                bowlingCenterName: "Championship Lanes",
                thumbnailUrl: thumbnailUrl,
                tournamentType: TournamentType.Trios)
        };

        var collectionResponse = new CollectionResponse<TournamentSummaryResponse>
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> apiResponse = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(year))
            .ReturnsAsync(apiResponse.ApiResponse);

        // Act
        ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>> result = await _sut.GetTournamentsInAYearAsync(year);

        // Assert
        result.IsError.ShouldBeFalse();
        var tournament = result.Value.First();
        tournament.Id.ShouldBe(tournamentId);
        tournament.Name.ShouldBe("2025 Championship");
        tournament.StartDate.ShouldBe(startDate);
        tournament.EndDate.ShouldBe(endDate);
        tournament.BowlingCenterId.ShouldBe(bowlingCenterId);
        tournament.BowlingCenterName.ShouldBe("Championship Lanes");
        tournament.ThumbnailUrl.ShouldBe(thumbnailUrl);
        tournament.TournamentType.ShouldBe(TournamentType.Trios.Name);
    }

    #endregion
}
