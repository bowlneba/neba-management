using System.Diagnostics;
using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Web.Server.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class ComponentLifecycleTelemetryTests : IDisposable
{
    private readonly ActivityListener _listener;

    public ComponentLifecycleTelemetryTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Neba.Web.Server.ComponentLifecycle",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose() => _listener.Dispose();

    [Fact(DisplayName = "RecordInitialization with async flag true completes successfully")]
    public void RecordInitialization_WithAsyncTrue_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "TestComponent";
        const double durationMs = 15.5;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, durationMs, isAsync: true));
    }

    [Fact(DisplayName = "RecordInitialization with async flag false completes successfully")]
    public void RecordInitialization_WithAsyncFalse_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "TestComponent";
        const double durationMs = 10.0;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, durationMs, isAsync: false));
    }

    [Fact(DisplayName = "RecordInitialization with default async parameter completes successfully")]
    public void RecordInitialization_WithDefaultAsync_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "TestComponent";
        const double durationMs = 20.3;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordInitialization with zero duration completes successfully")]
    public void RecordInitialization_WithZeroDuration_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "FastComponent";
        const double durationMs = 0.0;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordInitialization with very small duration completes successfully")]
    public void RecordInitialization_WithVerySmallDuration_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "QuickComponent";
        const double durationMs = 0.001;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordInitialization with large duration completes successfully")]
    public void RecordInitialization_WithLargeDuration_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "SlowComponent";
        const double durationMs = 5000.0;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordRender with first render true completes successfully")]
    public void RecordRender_WithFirstRenderTrue_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "TestComponent";
        const double durationMs = 25.5;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, durationMs, firstRender: true));
    }

    [Fact(DisplayName = "RecordRender with first render false completes successfully")]
    public void RecordRender_WithFirstRenderFalse_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "TestComponent";
        const double durationMs = 12.3;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, durationMs, firstRender: false));
    }

    [Fact(DisplayName = "RecordRender with zero duration completes successfully")]
    public void RecordRender_WithZeroDuration_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "InstantRenderComponent";
        const double durationMs = 0.0;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, durationMs, firstRender: true));
    }

    [Fact(DisplayName = "RecordRender with very small duration completes successfully")]
    public void RecordRender_WithVerySmallDuration_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "FastRenderComponent";
        const double durationMs = 0.5;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, durationMs, firstRender: false));
    }

    [Fact(DisplayName = "RecordRender with large duration completes successfully")]
    public void RecordRender_WithLargeDuration_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "SlowRenderComponent";
        const double durationMs = 3000.0;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, durationMs, firstRender: true));
    }

    [Fact(DisplayName = "RecordDisposal completes successfully")]
    public void RecordDisposal_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "TestComponent";

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordDisposal(componentName));
    }

    [Fact(DisplayName = "RecordDisposal can be called multiple times")]
    public void RecordDisposal_CalledMultipleTimes_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "RepeatedComponent";

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            Should.NotThrow(() => ComponentLifecycleTelemetry.RecordDisposal(componentName));
        }
    }

    [Fact(DisplayName = "StartActivity returns activity with correct name")]
    public void StartActivity_ReturnsActivityWithCorrectName()
    {
        // Arrange
        const string componentName = "TestComponent";
        const string lifecycleEvent = "Initialize";

        // Act
        using Activity? activity = ComponentLifecycleTelemetry.StartActivity(componentName, lifecycleEvent);

        // Assert
        activity.ShouldNotBeNull();
        activity.OperationName.ShouldBe("component.Initialize");
    }

    [Fact(DisplayName = "StartActivity sets component name tag")]
    public void StartActivity_SetsComponentNameTag()
    {
        // Arrange
        const string componentName = "MyComponent";
        const string lifecycleEvent = "Render";

        // Act
        using Activity? activity = ComponentLifecycleTelemetry.StartActivity(componentName, lifecycleEvent);

        // Assert
        activity.ShouldNotBeNull();
        activity.GetTagItem("component.name").ShouldBe(componentName);
    }

    [Fact(DisplayName = "StartActivity sets lifecycle event tag")]
    public void StartActivity_SetsLifecycleEventTag()
    {
        // Arrange
        const string componentName = "TestComponent";
        const string lifecycleEvent = "Dispose";

        // Act
        using Activity? activity = ComponentLifecycleTelemetry.StartActivity(componentName, lifecycleEvent);

        // Assert
        activity.ShouldNotBeNull();
        activity.GetTagItem("component.lifecycle.event").ShouldBe(lifecycleEvent);
    }

    [Fact(DisplayName = "StartActivity with Initialize event creates correct activity")]
    public void StartActivity_WithInitializeEvent_CreatesCorrectActivity()
    {
        // Arrange
        const string componentName = "InitComponent";
        const string lifecycleEvent = "Initialize";

        // Act
        using Activity? activity = ComponentLifecycleTelemetry.StartActivity(componentName, lifecycleEvent);

        // Assert
        activity.ShouldNotBeNull();
        activity.OperationName.ShouldBe("component.Initialize");
        activity.GetTagItem("component.name").ShouldBe(componentName);
        activity.GetTagItem("component.lifecycle.event").ShouldBe("Initialize");
    }

    [Fact(DisplayName = "StartActivity with Render event creates correct activity")]
    public void StartActivity_WithRenderEvent_CreatesCorrectActivity()
    {
        // Arrange
        const string componentName = "RenderComponent";
        const string lifecycleEvent = "Render";

        // Act
        using Activity? activity = ComponentLifecycleTelemetry.StartActivity(componentName, lifecycleEvent);

        // Assert
        activity.ShouldNotBeNull();
        activity.OperationName.ShouldBe("component.Render");
        activity.GetTagItem("component.name").ShouldBe(componentName);
        activity.GetTagItem("component.lifecycle.event").ShouldBe("Render");
    }

    [Fact(DisplayName = "StartActivity with Dispose event creates correct activity")]
    public void StartActivity_WithDisposeEvent_CreatesCorrectActivity()
    {
        // Arrange
        const string componentName = "DisposeComponent";
        const string lifecycleEvent = "Dispose";

        // Act
        using Activity? activity = ComponentLifecycleTelemetry.StartActivity(componentName, lifecycleEvent);

        // Assert
        activity.ShouldNotBeNull();
        activity.OperationName.ShouldBe("component.Dispose");
        activity.GetTagItem("component.name").ShouldBe(componentName);
        activity.GetTagItem("component.lifecycle.event").ShouldBe("Dispose");
    }

    [Fact(DisplayName = "Complete component lifecycle can be tracked")]
    public void CompleteComponentLifecycle_CanBeTracked()
    {
        // Arrange
        const string componentName = "LifecycleComponent";

        // Act & Assert - Initialize
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, 15.0, isAsync: true));

        // Render (first)
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, 10.0, firstRender: true));

        // Render (subsequent)
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, 8.0, firstRender: false));
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, 7.5, firstRender: false));

        // Dispose
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordDisposal(componentName));
    }

    [Fact(DisplayName = "Multiple components can be tracked concurrently")]
    public void MultipleComponents_CanBeTrackedConcurrently()
    {
        // Arrange
        const string component1 = "Component1";
        const string component2 = "Component2";
        const string component3 = "Component3";

        // Act & Assert
        Should.NotThrow(() =>
        {
            ComponentLifecycleTelemetry.RecordInitialization(component1, 10.0);
            ComponentLifecycleTelemetry.RecordInitialization(component2, 15.0);
            ComponentLifecycleTelemetry.RecordInitialization(component3, 20.0);

            ComponentLifecycleTelemetry.RecordRender(component1, 5.0, firstRender: true);
            ComponentLifecycleTelemetry.RecordRender(component2, 8.0, firstRender: true);

            ComponentLifecycleTelemetry.RecordDisposal(component1);

            ComponentLifecycleTelemetry.RecordRender(component3, 12.0, firstRender: true);
            ComponentLifecycleTelemetry.RecordRender(component2, 6.0, firstRender: false);

            ComponentLifecycleTelemetry.RecordDisposal(component2);
            ComponentLifecycleTelemetry.RecordDisposal(component3);
        });
    }

    [Fact(DisplayName = "RecordInitialization with long component name completes successfully")]
    public void RecordInitialization_WithLongComponentName_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "VeryLongComponentNameForTestingPurposesThatExceedsNormalLength";
        const double durationMs = 10.0;

        // Act & Assert
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordInitialization(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordRender sequence simulating multiple updates completes successfully")]
    public void RecordRender_MultipleUpdates_CompletesSuccessfully()
    {
        // Arrange
        const string componentName = "DynamicComponent";

        // Act & Assert - First render
        Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, 25.0, firstRender: true));

        // Subsequent renders
        for (int i = 0; i < 10; i++)
        {
            double duration = 10.0 + i * 0.5;
            Should.NotThrow(() => ComponentLifecycleTelemetry.RecordRender(componentName, duration, firstRender: false));
        }
    }

    [Fact(DisplayName = "StartActivity can be used with using statement")]
    public void StartActivity_WithUsingStatement_DisposesCorrectly()
    {
        // Arrange
        const string componentName = "UsingComponent";
        const string lifecycleEvent = "Initialize";

        // Act & Assert
        Should.NotThrow(() =>
        {
            using Activity? activity = ComponentLifecycleTelemetry.StartActivity(componentName, lifecycleEvent);
            activity.ShouldNotBeNull();
        });
    }
}
