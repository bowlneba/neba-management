using System.Net;
using ErrorOr;
using Neba.Contracts;
using Neba.Contracts.Website.Awards;
using Neba.Contracts.Website.Bowlers;
using Neba.Contracts.Website.Titles;
using Neba.Tests;
using Neba.Web.Server.History.Awards;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Refit;

namespace Neba.UnitTests.Services;

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
        var bowlerId = Guid.NewGuid();
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
        var bowlerId = Guid.NewGuid();
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
        var bowlerId = Guid.NewGuid();
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
        var bowlerId = Guid.NewGuid();
        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        ErrorOr<BowlerTitlesViewModel> result = await _sut.GetBowlerTitlesAsync(bowlerId);

        // Assert
        result.IsError.ShouldBeTrue();
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
