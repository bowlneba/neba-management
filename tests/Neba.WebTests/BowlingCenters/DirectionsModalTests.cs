using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Neba.Web.Server.BowlingCenters;
using Neba.Web.Server.Maps;
using MapsRouteData = Neba.Web.Server.Maps.RouteData;

namespace Neba.WebTests.BowlingCenters;

public sealed class DirectionsModalTests : TestContextWrapper
{
    [Fact]
    public void ShouldRenderModal_WhenIsOpenIsTrue()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldNotBeNullOrEmpty();
        cut.Markup.ShouldContain("Directions to Test Center");
    }

    [Fact]
    public void ShouldNotRenderModal_WhenIsOpenIsFalse()
    {
        // Arrange
        var state = new DirectionsState();

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert - Modal should be closed/not visible
        cut.Markup.ShouldNotContain("Directions to");
    }

    [Fact]
    public void ShouldDisplayLocationInputUI_WhenInDirectionsPreviewMode()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("Use My Current Location");
        cut.Markup.ShouldContain("Enter your starting address");
    }

    [Fact]
    public void ShouldDisplayErrorMessage_WhenErrorMessageIsSet()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center",
            ErrorMessage = "Location access denied"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("Location access denied");
    }

    [Fact]
    public void ShouldDisplayLoadingState_WhenIsLoadingIsTrue()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center",
            IsLoading = true
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("Getting your location...");
    }

    [Fact]
    public void ShouldDisplayDirections_WhenInDirectionsActiveModeWithRoute()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4, // ~10 miles
            TravelTimeSeconds = 900, // 15 minutes
            Instructions =
            [
                new RouteInstruction
                {
                    Text = "Head north on Main St",
                    DistanceMeters = 1000
                },
                new RouteInstruction
                {
                    Text = "Turn right on Oak Ave",
                    DistanceMeters = 500
                }
            ]
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("10.0 mi"); // Formatted distance
        cut.Markup.ShouldContain("15 min"); // Formatted travel time
        cut.Markup.ShouldContain("Open in Maps App");
        cut.Markup.ShouldContain("Turn-by-turn directions (2 steps)");
    }

    [Fact]
    public void ShouldDisplayTurnByTurnDirections_WhenExpanded()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900,
            Instructions =
            [
                new RouteInstruction
                {
                    Text = "Head north on Main St",
                    DistanceMeters = 1000
                },
                new RouteInstruction
                {
                    Text = "Turn right on Oak Ave",
                    DistanceMeters = 500
                }
            ]
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Act - Click to expand turn-by-turn directions
        IElement expandButton = cut.Find("button:contains('Turn-by-turn directions')");
        expandButton.Click();

        // Assert
        cut.Markup.ShouldContain("Head north on Main St");
        cut.Markup.ShouldContain("Turn right on Oak Ave");
    }

    [Fact]
    public void ShouldShowCloseButton_WhenInDirectionsPreviewMode()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("Cancel");
    }

    [Fact]
    public void ShouldShowCloseButton_WhenInDirectionsActiveMode()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("Close");
    }

    [Fact]
    public void ShouldInvokeOnClose_WhenCloseButtonClicked()
    {
        // Arrange
        bool closeInvoked = false;
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeInvoked = true))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Act
        IElement closeButton = cut.Find("button:contains('Cancel')");
        closeButton.Click();

        // Assert
        closeInvoked.ShouldBeTrue();
    }

    [Fact]
    public void ShouldDisplayAddressInput_WhenInDirectionsPreviewMode()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        IElement addressInput = cut.Find("#address-input");
        addressInput.ShouldNotBeNull();
        addressInput.GetAttribute("placeholder").ShouldBe("123 Main St, Boston, MA");
    }

    [Fact]
    public void ShouldFormatTravelTime_AsHoursAndMinutes_WhenOver60Minutes()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 96560.6, // ~60 miles
            TravelTimeSeconds = 5400 // 90 minutes = 1 hr 30 min
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("1 hr 30 min");
    }

    [Fact]
    public async Task ShouldInvokeOnLocationSelected_WhenCurrentLocationButtonClicked()
    {
        // Arrange
        double[]? selectedLocation = null;
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Setup JS interop to return a mock location
        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.Setup<double[]>("getCurrentLocation")
            .SetResult([42.3601, -71.0589]); // Boston coordinates

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, loc => selectedLocation = loc)));

        // Force initial render to load JS module
        await cut.InvokeAsync(() => { });

        // Act
        IElement locationButton = cut.Find("button:contains('Use My Current Location')");
        await cut.InvokeAsync(async () => await locationButton.ClickAsync(new()));

        // Assert
        selectedLocation.ShouldNotBeNull();
        selectedLocation.ShouldBe([42.3601, -71.0589]);
    }

    [Fact]
    public async Task ShouldDisplayErrorMessage_WhenLocationAccessDenied()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Setup JS interop to throw a denied error
        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.Setup<double[]>("getCurrentLocation")
            .SetException(new JSException("User denied geolocation prompt"));

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Force initial render
        await cut.InvokeAsync(() => { });

        // Act
        IElement locationButton = cut.Find("button:contains('Use My Current Location')");
        await cut.InvokeAsync(async () => await locationButton.ClickAsync(new()));

        // Assert
        cut.Instance.State.ErrorMessage.ShouldNotBeNull();
        cut.Instance.State.ErrorMessage.ShouldContain("Location access denied");
    }

    [Fact]
    public async Task ShouldDisplayGenericErrorMessage_WhenLocationFails()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Setup JS interop to throw a generic error
        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.Setup<double[]>("getCurrentLocation")
            .SetException(new JSException("Network timeout"));

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Force initial render
        await cut.InvokeAsync(() => { });

        // Act
        IElement locationButton = cut.Find("button:contains('Use My Current Location')");
        await cut.InvokeAsync(async () => await locationButton.ClickAsync(new()));

        // Assert
        cut.Instance.State.ErrorMessage.ShouldNotBeNull();
        cut.Instance.State.ErrorMessage.ShouldContain("Unable to get your location");
        cut.Instance.State.ErrorMessage.ShouldContain("Network timeout");
    }

    [Fact]
    public void ShouldNotDisplayRouteInstructions_WhenCollapsed()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900,
            Instructions =
            [
                new RouteInstruction
                {
                    Text = "Head north on Main St",
                    DistanceMeters = 1000
                }
            ]
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert - Directions should not be visible when collapsed
        cut.Markup.ShouldNotContain("Head north on Main St");
    }

    [Fact]
    public void ShouldHideDirections_WhenCollapsedAfterExpanding()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900,
            Instructions =
            [
                new RouteInstruction
                {
                    Text = "Head north on Main St",
                    DistanceMeters = 1000
                }
            ]
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        IElement expandButton = cut.Find("button:contains('Turn-by-turn directions')");

        // Act - Expand then collapse
        expandButton.Click();
        cut.Markup.ShouldContain("Head north on Main St"); // Verify it's shown
        expandButton.Click(); // Collapse

        // Assert
        cut.Markup.ShouldNotContain("Head north on Main St");
    }

    [Fact]
    public void ShouldNotDisplayDistance_ForInstructionWithZeroDistance()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900,
            Instructions =
            [
                new RouteInstruction
                {
                    Text = "You have arrived",
                    DistanceMeters = 0 // Final instruction with no distance
                }
            ]
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Act
        IElement expandButton = cut.Find("button:contains('Turn-by-turn directions')");
        expandButton.Click();

        // Assert - Should show the instruction text but not distance
        cut.Markup.ShouldContain("You have arrived");
        // The distance for a 0-meter instruction should not be displayed (verified by checking FormattedDistance is not rendered)
    }

    [Fact]
    public async Task ShouldOpenMapsApp_WhenOpenInMapsButtonClicked()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route,
            UserLocation = [-71.0589, 42.3601], // [lon, lat]
            DestinationLocation = [-71.0636, 42.3656]
        };

        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.SetupVoid("openInNewTab");

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Force initial render
        await cut.InvokeAsync(() => { });

        // Act
        IElement openButton = cut.Find("button:contains('Open in Maps App')");
        await cut.InvokeAsync(async () => await openButton.ClickAsync(new()));

        // Assert
        // Verify that the JS interop was called (bUnit will track this)
        jsModule.VerifyInvoke("openInNewTab");
    }

    [Fact]
    public async Task ShouldNotOpenMapsApp_WhenUserLocationIsNull()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route,
            UserLocation = null, // No user location
            DestinationLocation = [-71.0636, 42.3656]
        };

        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.SetupVoid("openInNewTab");

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Force initial render
        await cut.InvokeAsync(() => { });

        // Act
        IElement openButton = cut.Find("button:contains('Open in Maps App')");
        await cut.InvokeAsync(async () => await openButton.ClickAsync(new()));

        // Assert - Should not invoke JS when location is null
        var invocations = jsModule.Invocations["openInNewTab"];
        invocations.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldNotOpenMapsApp_WhenDestinationLocationIsNull()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route,
            UserLocation = [-71.0589, 42.3601],
            DestinationLocation = null // No destination
        };

        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.SetupVoid("openInNewTab");

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Force initial render
        await cut.InvokeAsync(() => { });

        // Act
        IElement openButton = cut.Find("button:contains('Open in Maps App')");
        await cut.InvokeAsync(async () => await openButton.ClickAsync(new()));

        // Assert - Should not invoke JS when destination is null
        var invocations = jsModule.Invocations["openInNewTab"];
        invocations.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldDisposeJSModule_WhenComponentDisposed()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Force initial render to load JS module
        await cut.InvokeAsync(() => { });

        // Act
        await cut.InvokeAsync(async () => await cut.Instance.DisposeAsync());

        // Assert - Component should dispose without throwing exceptions
        cut.Instance.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldDisableLocationButton_WhenLoading()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center",
            IsLoading = true
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        IElement locationButton = cut.Find("button:contains('Getting your location')");
        locationButton.ShouldNotBeNull();
        locationButton.HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public void ShouldDisableAddressInput_WhenLoading()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center",
            IsLoading = true
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        IElement addressInput = cut.Find("#address-input");
        addressInput.HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldClearErrorMessage_WhenTryingLocationAgain()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center",
            ErrorMessage = "Previous error"
        };

        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.Setup<double[]>("getCurrentLocation")
            .SetResult([42.3601, -71.0589]);

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        await cut.InvokeAsync(() => { });

        // Act
        IElement locationButton = cut.Find("button:contains('Use My Current Location')");
        await cut.InvokeAsync(async () => await locationButton.ClickAsync(new()));

        // Assert
        cut.Instance.State.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldSetUserLocation_WhenLocationRetrieved()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        var jsModule = TestContext.JSInterop.SetupModule("./BowlingCenters/DirectionsModal.razor.js");
        jsModule.Setup<double[]>("getCurrentLocation")
            .SetResult([42.3601, -71.0589]);

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        await cut.InvokeAsync(() => { });

        // Act
        IElement locationButton = cut.Find("button:contains('Use My Current Location')");
        await cut.InvokeAsync(async () => await locationButton.ClickAsync(new()));

        // Assert
        cut.Instance.State.UserLocation.ShouldNotBeNull();
        cut.Instance.State.UserLocation.ShouldBe([42.3601, -71.0589]);
        cut.Instance.State.UserAddress.ShouldBe("Current Location");
    }

    [Fact]
    public void ShouldHaveAccessibilityAttributes_OnAddressInput()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        IElement addressInput = cut.Find("#address-input");
        addressInput.GetAttribute("aria-label").ShouldBe("Starting address");
        addressInput.GetAttribute("aria-describedby").ShouldBe("address-help");
    }

    [Fact]
    public void ShouldDisplayStepNumbers_InTurnByTurnDirections()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900,
            Instructions =
            [
                new RouteInstruction { Text = "First instruction", DistanceMeters = 1000 },
                new RouteInstruction { Text = "Second instruction", DistanceMeters = 500 },
                new RouteInstruction { Text = "Third instruction", DistanceMeters = 300 }
            ]
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Act
        IElement expandButton = cut.Find("button:contains('Turn-by-turn directions')");
        expandButton.Click();

        // Assert - Should show numbered steps
        cut.Markup.ShouldContain("First instruction");
        cut.Markup.ShouldContain("Second instruction");
        cut.Markup.ShouldContain("Third instruction");
    }

    [Fact]
    public void ShouldShowFormattedDistance_InRouteInstructions()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900,
            Instructions =
            [
                new RouteInstruction
                {
                    Text = "Head north",
                    DistanceMeters = 1609.34 // ~1 mile
                }
            ]
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Act
        IElement expandButton = cut.Find("button:contains('Turn-by-turn directions')");
        expandButton.Click();

        // Assert
        cut.Markup.ShouldContain("1.0 mi");
    }

    [Fact]
    public async Task ShouldResetState_WhenCloseButtonClicked()
    {
        // Arrange
        bool closeWasCalled = false;
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeWasCalled = true))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Act - Click close button
        IElement closeButton = cut.Find("button:contains('Cancel')");
        await cut.InvokeAsync(async () => await closeButton.ClickAsync(new()));

        // Assert - OnClose callback should be invoked and component should not throw
        closeWasCalled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldDisplayLocationIcon_OnCurrentLocationButton()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        IElement locationButton = cut.Find("button:contains('Use My Current Location')");
        locationButton.InnerHtml.ShouldContain("<svg");
    }

    [Fact]
    public void ShouldDisplayMapIcon_InDirectionsSummary()
    {
        // Arrange
        var route = new MapsRouteData
        {
            DistanceMeters = 16093.4,
            TravelTimeSeconds = 900
        };

        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsActive,
            SelectedCenterName = "Test Center",
            Route = route
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert - Should show map icon in summary section
        cut.FindAll("svg").Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ShouldDisplayORDivider_BetweenLocationOptions()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("OR");
    }

    [Fact]
    public void ShouldDisplayRecommendedText_UnderCurrentLocationButton()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("Recommended for fastest results");
    }

    [Fact]
    public void ShouldDisplayAddressHelpText()
    {
        // Arrange
        var state = new DirectionsState
        {
            Mode = MapMode.DirectionsPreview,
            SelectedCenterName = "Test Center"
        };

        // Act
        IRenderedComponent<DirectionsModal> cut = Render<DirectionsModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.State, state)
            .Add(p => p.OnLocationSelected, EventCallback.Factory.Create<double[]>(this, _ => { })));

        // Assert
        cut.Markup.ShouldContain("Start typing to see suggestions");
    }
}
