using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
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
}
