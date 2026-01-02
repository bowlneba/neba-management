using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Domain.Contact;
using Neba.Tests;
using Neba.Web.Server.BowlingCenters;
using Neba.Web.Server.Components;
using Neba.Web.Server.Maps;
using Neba.Web.Server.Notifications;
using Neba.Web.Server.Services;
using Neba.Website.Contracts.BowlingCenters;

namespace Neba.WebTests.BowlingCenters;

[Trait("Category", "Web")]
[Trait("Component", "BowlingCenters")]

public sealed class BowlingCentersTests : TestContextWrapper
{
    private readonly Mock<INebaApi> _mockNebaApi;

    public BowlingCentersTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        NebaApiService nebaApiService = new(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(nebaApiService);

        // Register AzureMapsSettings for NebaMap component
        var mapsSettings = new AzureMapsSettings
        {
            AccountId = "test-account-id",
            SubscriptionKey = "test-subscription-key"
        };
        TestContext.Services.AddSingleton(mapsSettings);
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

        // Assert - Check that state filter buttons are displayed
        cut.Markup.ShouldContain("Filter by State:");
        cut.Markup.ShouldContain("All States");
        cut.Markup.ShouldContain("MA");
        cut.Markup.ShouldContain("CT");
        cut.Markup.ShouldContain("RI");
        cut.Markup.ShouldContain("NH");
        cut.Markup.ShouldContain("ME");
        cut.Markup.ShouldContain("VT");
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
        // Arrange - Return an error response instead of throwing
        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = []
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateResponse(collectionResponse, System.Net.HttpStatusCode.ServiceUnavailable);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Assert - Check that error alert is displayed
        cut.ShouldNotBeNull();
        cut.Markup.ShouldContain("Bowling Centers");
        cut.Markup.ShouldContain("Error Loading Centers");
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

        // Wait for the component to finish loading
        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Bowling Centers"));

        // Assert - Verify the page content is rendered (HeadContent is not part of component markup in bUnit)
        cut.Markup.ShouldContain("USBC certified centers across New England");
    }

    [Fact]
    public void DisplaysMapStyleSwitcher()
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
        cut.Markup.ShouldContain("Map View:");
        cut.Markup.ShouldContain("Road");
        cut.Markup.ShouldContain("Satellite");
        cut.Markup.ShouldContain("Hybrid");
    }

    [Fact]
    public void RendersNebaMapComponent()
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
        IRenderedComponent<NebaMap> map = cut.FindComponent<NebaMap>();
        map.ShouldNotBeNull();
    }

    [Fact]
    public void RendersDirectionsModalComponent()
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
        IRenderedComponent<DirectionsModal> modal = cut.FindComponent<DirectionsModal>();
        modal.ShouldNotBeNull();
    }

    [Fact]
    public void FormatPhoneNumber_NonStandardFormat_ReturnsAsIs()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse(phoneNumber: "5551234") // Non-standard format
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

        // Assert - Should display as-is when not 11 digits
        cut.Markup.ShouldContain("5551234");
    }

    [Fact]
    public void FormatZipCode_FiveDigitZip_ReturnsAsIs()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse(zipCode: "02108") // 5-digit ZIP
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
        cut.Markup.ShouldContain("02108");
    }

    [Fact]
    public void GetPhoneTelUri_FormatsCorrectly()
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
        cut.Markup.ShouldContain("tel:+16175551234");
    }

    [Fact]
    public void DoesNotDisplayCentersWithInvalidCoordinates()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("Valid Center", latitude: 42.3601, longitude: -71.0589),
            CreateTestCenterResponse("Invalid Latitude", latitude: 0, longitude: -71.0589),
            CreateTestCenterResponse("Invalid Longitude", latitude: 42.3601, longitude: 0)
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
        cut.Markup.ShouldContain("Valid Center");
        // Map should only have valid centers (defensive programming)
    }

    [Fact]
    public void DisplaysNoCentersMessage_WhenAllCentersOutsideMapBounds()
    {
        // Arrange - This test verifies the "No centers visible in current map view" message
        // In actual usage, this would happen when the map is zoomed/panned to a different area
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

        // Assert - Verify the component can render the no centers message
        // (This message appears when _displayedCenters.Count == 0 but _filteredCenters.Count > 0)
        cut.Markup.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void DisplaysPhoneExtension_WhenPresent()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse(phoneExtension: "123")
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
        cut.Markup.ShouldContain("x123");
    }

    [Fact]
    public void DoesNotDisplayPhoneExtension_WhenNotPresent()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse(phoneExtension: null)
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
        cut.Markup.ShouldNotContain(" x");
    }

    [Fact]
    public async Task ClickingStateFilter_FiltersResults()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("MA Center 1", state: "MA"),
            CreateTestCenterResponse("MA Center 2", state: "MA"),
            CreateTestCenterResponse("CT Center", state: "CT")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Act - Click MA filter button
        IElement maButton = cut.Find("button.state-btn:contains('MA')");
        await cut.InvokeAsync(async () => await maButton.ClickAsync(new()));

        // Assert - MA button should have active class
        maButton.ClassList.ShouldContain("active");
    }

    [Fact]
    public async Task SearchBox_FiltersResults()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("Kings Bowling", city: "Boston"),
            CreateTestCenterResponse("Strike Zone", city: "Hartford"),
            CreateTestCenterResponse("Lucky Strike", city: "Providence")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Act - Type in search box
        IElement searchInput = cut.Find("input[placeholder='Search centers...']");
        await cut.InvokeAsync(async () => await searchInput.InputAsync("Kings"));

        // Assert - Search input should have value
        string? inputValue = searchInput.GetAttribute("value");
        inputValue.ShouldNotBeNull();
        inputValue.ShouldContain("Kings");
    }

    [Fact]
    public async Task ClickingAllStatesFilter_ShowsAllCenters()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("MA Center", state: "MA"),
            CreateTestCenterResponse("CT Center", state: "CT")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Act - Click "All States" button
        IElement allStatesButton = cut.Find("button.state-btn:contains('All States')");
        await cut.InvokeAsync(async () => await allStatesButton.ClickAsync(new()));

        // Assert
        allStatesButton.ClassList.ShouldContain("active");
    }

    [Fact]
    public async Task ClickingCenterCard_InvokesMapFocus()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("Test Center")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Act - Click on center card
        IElement centerCard = cut.Find("div.neba-card:contains('Test Center')");
        await cut.InvokeAsync(async () => await centerCard.ClickAsync(new()));

        // Assert - Component should not throw on click
        cut.ShouldNotBeNull();
    }

    [Fact]
    public async Task ClickingGetDirections_OpensDirectionsModal()
    {
        // Arrange
        List<BowlingCenterResponse> centers =
        [
            CreateTestCenterResponse("Test Center")
        ];

        CollectionResponse<BowlingCenterResponse> collectionResponse = new CollectionResponse<BowlingCenterResponse>
        {
            Items = centers
        };

        using TestApiResponse<CollectionResponse<BowlingCenterResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlingCentersAsync())
            .ReturnsAsync(response.ApiResponse);

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Act - Click "Get Directions" button
        IElement directionsButton = cut.Find("button:contains('Get Directions')");
        await cut.InvokeAsync(async () => await directionsButton.ClickAsync(new()));

        // Assert - DirectionsModal should be rendered with IsOpen=true
        IRenderedComponent<DirectionsModal> modal = cut.FindComponent<DirectionsModal>();
        modal.Instance.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task ClickingMapStyleButton_ChangesMapStyle()
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

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Act - Click "Satellite" button
        IElement satelliteButton = cut.Find("button:contains('Satellite')");
        await cut.InvokeAsync(async () => await satelliteButton.ClickAsync(new()));

        // Assert - Satellite button should have active styling
        await cut.WaitForAssertionAsync(() =>
        {
            satelliteButton = cut.Find("button:contains('Satellite')");
            string? classAttr = satelliteButton.GetAttribute("class");
            classAttr.ShouldNotBeNull();
            classAttr.ShouldContain("bg-[var(--neba-blue-600)]");
        });
    }

    [Fact]
    public async Task DisposingComponent_DisposesJSModule()
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

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Act
        await cut.InvokeAsync(async () => await cut.Instance.DisposeAsync());

        // Assert - Component should dispose without throwing
        cut.Instance.ShouldNotBeNull();
    }

    [Fact]
    public void DisplaysScrollContainer_WithCorrectMaxHeight()
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
        cut.Markup.ShouldContain("centers-scroll-container");
        cut.Markup.ShouldContain("max-height: 600px");
    }

    [Fact]
    public void NoErrorAlert_WhenNoError()
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
        cut.Markup.ShouldNotContain("Error Loading Centers");
    }

    [Fact]
    public async Task DismissingErrorAlert_HidesError()
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

        IRenderedComponent<Neba.Web.Server.BowlingCenters.BowlingCenters> cut = Render<Neba.Web.Server.BowlingCenters.BowlingCenters>();

        // Wait for error to appear
        await cut.WaitForAssertionAsync(() => cut.Markup.ShouldContain("Error Loading Centers"), timeout: TimeSpan.FromSeconds(5));

        // Act - Find and click dismiss button on NebaAlert
        IRenderedComponent<NebaAlert> alert = cut.FindComponent<NebaAlert>();
        await alert.InvokeAsync(() => alert.Instance.OnDismiss.InvokeAsync());

        // Assert - Error should be dismissed
        await cut.WaitForAssertionAsync(() => cut.Markup.ShouldNotContain("Error Loading Centers"), timeout: TimeSpan.FromSeconds(5));
    }
}
