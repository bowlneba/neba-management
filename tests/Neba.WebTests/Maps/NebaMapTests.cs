using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Neba.Web.Server.Maps;
using MapsRouteData = Neba.Web.Server.Maps.RouteData;

namespace Neba.WebTests.Maps;

public sealed class NebaMapTests : TestContextWrapper
{
    public NebaMapTests()
    {
        // Mock the AzureMapsSettings service
        var mapsSettings = new AzureMapsSettings
        {
            AccountId = "test-account-id",
            SubscriptionKey = "test-subscription-key"
        };
        TestContext.Services.AddSingleton(mapsSettings);
    }

    private static NebaMapLocation CreateTestLocation(
        string id = "test-location-1",
        string title = "Test Location",
        string description = "123 Main St",
        double latitude = 42.3601,
        double longitude = -71.0589,
        Dictionary<string, object>? metadata = null)
    {
        return new NebaMapLocation(
            id,
            title,
            description,
            latitude,
            longitude,
            metadata);
    }

    [Fact]
    public void ShouldRenderMapContainer()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ShouldApplyDefaultHeight()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.Markup.ShouldContain("height: 600px");
    }

    [Fact]
    public void ShouldApplyCustomHeight()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations)
            .Add(p => p.Height, "800px"));

        // Assert
        cut.Markup.ShouldContain("height: 800px");
    }

    [Fact]
    public void ShouldApplyDefaultWidth()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.Markup.ShouldContain("width: 100%");
    }

    [Fact]
    public void ShouldApplyCustomWidth()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations)
            .Add(p => p.Width, "500px"));

        // Assert
        cut.Markup.ShouldContain("width: 500px");
    }

    [Fact]
    public void ShouldApplyDefaultCssClass()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.Markup.ShouldContain("class=\"neba-card\"");
    }

    [Fact]
    public void ShouldApplyCustomCssClass()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations)
            .Add(p => p.CssClass, "custom-map-class"));

        // Assert
        cut.Markup.ShouldContain("class=\"custom-map-class\"");
    }

    [Fact]
    public void ShouldRenderWithEmptyLocations()
    {
        // Arrange & Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, Array.Empty<NebaMapLocation>()));

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ShouldRenderWithMultipleLocations()
    {
        // Arrange
        List<NebaMapLocation> locations =
        [
            CreateTestLocation("loc-1", "Location 1", "Address 1", 42.3601, -71.0589),
            CreateTestLocation("loc-2", "Location 2", "Address 2", 42.3500, -71.0500),
            CreateTestLocation("loc-3", "Location 3", "Address 3", 42.3700, -71.0700)
        ];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ShouldIncludeAzureMapsScriptReferences()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.Markup.ShouldContain("atlas.microsoft.com");
    }

    [Fact]
    public void ShouldIncludeLocalJavaScriptModule()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.Markup.ShouldContain("./Maps/NebaMap.razor.js");
    }

    [Fact]
    public void ShouldUseDefaultCenter_WhenNotProvided()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert - Default center is Boston, MA [-71.0589, 42.3601]
        cut.Instance.Center.ShouldNotBeNull();
        cut.Instance.Center.Length.ShouldBe(2);
        cut.Instance.Center[0].ShouldBe(-71.0589);
        cut.Instance.Center[1].ShouldBe(42.3601);
    }

    [Fact]
    public void ShouldUseCustomCenter_WhenProvided()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];
        double[] customCenter = [-72.0, 43.0];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations)
            .Add(p => p.Center, customCenter));

        // Assert
        cut.Instance.Center.ShouldBe(customCenter);
    }

    [Fact]
    public void ShouldUseDefaultZoom_WhenNotProvided()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.Instance.Zoom.ShouldBe(7);
    }

    [Fact]
    public void ShouldUseCustomZoom_WhenProvided()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations)
            .Add(p => p.Zoom, 10));

        // Assert
        cut.Instance.Zoom.ShouldBe(10);
    }

    [Fact]
    public void ShouldEnableClustering_ByDefault()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.Instance.EnableClustering.ShouldBeTrue();
    }

    [Fact]
    public void ShouldDisableClustering_WhenSetToFalse()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations)
            .Add(p => p.EnableClustering, false));

        // Assert
        cut.Instance.EnableClustering.ShouldBeFalse();
    }

    [Fact]
    public void ShouldRenderLocation_WithMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["street"] = "123 Main St",
            ["city"] = "Boston",
            ["state"] = "MA"
        };

        List<NebaMapLocation> locations = [CreateTestLocation(metadata: metadata)];

        // Act
        IRenderedComponent<NebaMap> cut = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert
        cut.ShouldNotBeNull();
        cut.Instance.Locations.ShouldNotBeEmpty();
        NebaMapLocation firstLocation = cut.Instance.Locations.First();
        firstLocation.Metadata.ShouldNotBeNull();
        firstLocation.Metadata["street"].ShouldBe("123 Main St");
    }

    [Fact]
    public void ShouldGenerateUniqueContainerId()
    {
        // Arrange
        List<NebaMapLocation> locations = [CreateTestLocation()];

        // Act
        IRenderedComponent<NebaMap> cut1 = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        IRenderedComponent<NebaMap> cut2 = Render<NebaMap>(parameters => parameters
            .Add(p => p.Locations, locations));

        // Assert - Each instance should have a unique container ID
        cut1.Markup.ShouldContain("id=\"neba-map-");
        cut2.Markup.ShouldContain("id=\"neba-map-");
        cut1.Markup.ShouldNotBe(cut2.Markup); // IDs should be different
    }

    [Fact]
    public void MapBounds_ContainsMethod_ReturnsTrueForPointInsideBounds()
    {
        // Arrange
        MapBounds bounds = new MapBounds(
            North: 43.0,
            South: 42.0,
            East: -70.0,
            West: -72.0);

        // Act & Assert
        bounds.Contains(42.5, -71.0).ShouldBeTrue();
    }

    [Fact]
    public void MapBounds_ContainsMethod_ReturnsFalseForPointOutsideBounds()
    {
        // Arrange
        MapBounds bounds = new MapBounds(
            North: 43.0,
            South: 42.0,
            East: -70.0,
            West: -72.0);

        // Act & Assert
        bounds.Contains(44.0, -71.0).ShouldBeFalse(); // North of bounds
        bounds.Contains(41.0, -71.0).ShouldBeFalse(); // South of bounds
        bounds.Contains(42.5, -69.0).ShouldBeFalse(); // East of bounds
        bounds.Contains(42.5, -73.0).ShouldBeFalse(); // West of bounds
    }

    [Fact]
    public void MapBounds_ContainsMethod_ReturnsTrueForPointOnBoundary()
    {
        // Arrange
        MapBounds bounds = new MapBounds(
            North: 43.0,
            South: 42.0,
            East: -70.0,
            West: -72.0);

        // Act & Assert
        bounds.Contains(43.0, -71.0).ShouldBeTrue(); // On north boundary
        bounds.Contains(42.0, -71.0).ShouldBeTrue(); // On south boundary
        bounds.Contains(42.5, -70.0).ShouldBeTrue(); // On east boundary
        bounds.Contains(42.5, -72.0).ShouldBeTrue(); // On west boundary
    }

    [Fact]
    public void RouteData_FormattedDistance_DisplaysInMiles()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4 // ~10 miles
        };

        // Act & Assert
        route.FormattedDistance.ShouldBe("10.0 mi");
    }

    [Fact]
    public void RouteData_FormattedTravelTime_DisplaysInMinutes_WhenLessThan60()
    {
        // Arrange
        var route = new MapsRouteData
        {
            TravelTimeSeconds = 1800 // 30 minutes
        };

        // Act & Assert
        route.FormattedTravelTime.ShouldBe("30 min");
    }

    [Fact]
    public void RouteData_FormattedTravelTime_DisplaysInHoursAndMinutes_When60OrMore()
    {
        // Arrange
        var route = new MapsRouteData
        {
            TravelTimeSeconds = 5400 // 90 minutes = 1 hr 30 min
        };

        // Act & Assert
        route.FormattedTravelTime.ShouldBe("1 hr 30 min");
    }

    [Fact]
    public void RouteInstruction_FormattedDistance_DisplaysInFeet_WhenLessThanTenthMile()
    {
        // Arrange
        var instruction = new RouteInstruction
        {
            DistanceMeters = 100 // ~328 feet
        };

        // Act & Assert
        instruction.FormattedDistance.ShouldBe("328 ft");
    }

    [Fact]
    public void RouteInstruction_FormattedDistance_DisplaysInMiles_WhenTenthMileOrMore()
    {
        // Arrange
        var instruction = new RouteInstruction
        {
            DistanceMeters = 1000 // ~0.6 miles
        };

        // Act & Assert
        instruction.FormattedDistance.ShouldBe("0.6 mi");
    }

    [Fact]
    public void DirectionsState_Reset_ClearsAllProperties()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterId = "center-1",
            SelectedCenterName = "Test Center",
            UserLocation = [42.0, -71.0],
            UserAddress = "123 Main St",
            DestinationLocation = [42.5, -71.5],
            Route = new MapsRouteData { DistanceMeters = 1000 },
            IsLoading = true,
            ErrorMessage = "Test error"
        };

        // Act
        state.Reset();

        // Assert
        state.Mode.ShouldBe(MapMode.Overview);
        state.SelectedCenterId.ShouldBeNull();
        state.SelectedCenterName.ShouldBeNull();
        state.UserLocation.ShouldBeNull();
        state.UserAddress.ShouldBeNull();
        state.DestinationLocation.ShouldBeNull();
        state.Route.ShouldBeNull();
        state.IsLoading.ShouldBeFalse();
        state.ErrorMessage.ShouldBeNull();
    }
}
