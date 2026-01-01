using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Domain.Contact;
using Neba.Tests;
using Neba.Web.Server.BowlingCenters;
using Neba.Web.Server.Components;
using Neba.Web.Server.Services;
using Neba.Website.Contracts.BowlingCenters;

namespace Neba.WebTests.BowlingCenters;

public sealed class BowlingCentersTests : TestContextWrapper
{
    private readonly Mock<INebaApi> _mockNebaApi;
    private readonly NebaApiService _nebaApiService;

    public BowlingCentersTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        _nebaApiService = new NebaApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaApiService);
    }

    private static BowlingCenterResponse CreateTestCenterResponse(
        string name = "Test Center",
        string street = "123 Main St",
        string? unit = null,
        string city = "Boston",
        string state = "MA",
        string zipCode = "02108",
        string phoneNumber = "16175551234",
        string? phoneExtension = null,
        double latitude = 42.3601,
        double longitude = -71.0589,
        bool isClosed = false)
    {
        return new BowlingCenterResponse
        {
            Name = name,
            Street = street,
            Unit = unit,
            City = city,
            State = UsState.FromValue(state),
            ZipCode = zipCode,
            PhoneNumber = phoneNumber,
            PhoneExtension = phoneExtension,
            Latitude = latitude,
            Longitude = longitude,
            IsClosed = isClosed
        };
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_LoadsCenters()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("Center 1", city: "Boston", state: "MA"),
            CreateTestCenterResponse("Center 2", city: "Hartford", state: "CT"),
            CreateTestCenterResponse("Center 3", city: "Providence", state: "RI")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();
        cut.Markup.ShouldContain("Bowling Centers");
        cut.Markup.ShouldContain("USBC certified centers across New England");
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_DisplaysCenterCards()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("Kings Bowling", city: "Boston", state: "MA", phoneNumber: "16175551234", phoneExtension: "100"),
            CreateTestCenterResponse("Strike Zone", city: "Hartford", state: "CT")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("Kings Bowling");
        cut.Markup.ShouldContain("Strike Zone");
        cut.Markup.ShouldContain("Boston");
        cut.Markup.ShouldContain("Hartford");
        cut.Markup.ShouldContain("(617) 555-1234");
        cut.Markup.ShouldContain("x100");
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_DisplaysStateFilters()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse()
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("All States");
        cut.Markup.ShouldContain(">MA<");
        cut.Markup.ShouldContain(">CT<");
        cut.Markup.ShouldContain(">RI<");
        cut.Markup.ShouldContain(">NH<");
        cut.Markup.ShouldContain(">ME<");
        cut.Markup.ShouldContain(">VT<");
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_DisplaysSearchBox()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse()
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        IElement searchInput = cut.Find("input[placeholder='Search centers...']");
        searchInput.ShouldNotBeNull();
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_DisplaysResultsCount()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("Center 1"),
            CreateTestCenterResponse("Center 2"),
            CreateTestCenterResponse("Center 3")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("Showing");
        cut.Markup.ShouldContain("of 3 centers");
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_DisplaysGetDirectionsButton()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse()
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("Get Directions");
    }

    [Fact]
    public void OnInitializedAsync_ApiError_DisplaysErrorAlert()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ThrowsAsync(new InvalidOperationException("API connection failed"));

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldContain("Error Loading Centers");
        cut.Markup.ShouldContain("Failed to load bowling centers");
        cut.Markup.ShouldContain("API connection failed");
    }

    [Fact]
    public void OnInitializedAsync_ApiReturnsError_DisplaysErrorAlert()
    {
        // Arrange
        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = []
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateResponse(collectionResponse, System.Net.HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("Error Loading Centers");
    }

    [Fact]
    public void Render_IncludesLoadingIndicator()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse()
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.ShouldNotBeNull();
    }

    [Fact]
    public void FormatPhoneNumber_FormatsCorrectly()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse(phoneNumber: "16175551234")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("(617) 555-1234");
    }

    [Fact]
    public void FormatZipCode_FormatsNineDigitZip()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse(zipCode: "021081234")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("02108-1234");
    }

    [Fact]
    public void DisplaysUnit_WhenCenterHasUnit()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse(unit: "Suite 200")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert
        cut.Markup.ShouldContain("Suite 200");
    }

    [Fact]
    public void RendersPageTitle_AndMetaDescription()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse()
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert - Page title and meta tags render through Blazor components
        cut.Markup.ShouldContain("Browse USBC certified bowling centers across New England");
    }
}
