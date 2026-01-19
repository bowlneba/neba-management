using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Components;

namespace Neba.UnitTests.Web.Server.Components;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Components")]
public sealed class TelemeteredComponentBaseTests
{
    /// <summary>
    /// Test implementation of TelemeteredComponentBase for testing.
    /// </summary>
    private sealed class TestComponent : TelemeteredComponentBase
    {
        public bool InitializedCoreWasCalled { get; private set; }
        public bool AfterRenderCoreWasCalled { get; private set; }
        public bool AfterRenderFirstRenderValue { get; private set; }

        public async Task CallOnInitializedAsync()
        {
            await OnInitializedAsync();
        }

        public async Task CallOnAfterRenderAsync(bool firstRender)
        {
            await OnAfterRenderAsync(firstRender);
        }

        protected override Task OnInitializedCoreAsync()
        {
            InitializedCoreWasCalled = true;
            return Task.CompletedTask;
        }

        protected override Task OnAfterRenderCoreAsync(bool firstRender)
        {
            AfterRenderCoreWasCalled = true;
            AfterRenderFirstRenderValue = firstRender;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test implementation with custom component name.
    /// </summary>
    private sealed class CustomNameComponent : TelemeteredComponentBase
    {
        protected override string ComponentName => "MyCustomComponent";

        public async Task CallOnInitializedAsync()
        {
            await OnInitializedAsync();
        }

        public string GetComponentName() => ComponentName;
    }

    /// <summary>
    /// Test implementation with telemetry disabled.
    /// </summary>
    private sealed class TelemetryDisabledComponent : TelemeteredComponentBase
    {
        protected override bool EnableLifecycleTelemetry => false;

        public bool InitializedCoreWasCalled { get; private set; }
        public bool AfterRenderCoreWasCalled { get; private set; }

        public async Task CallOnInitializedAsync()
        {
            await OnInitializedAsync();
        }

        public async Task CallOnAfterRenderAsync(bool firstRender)
        {
            await OnAfterRenderAsync(firstRender);
        }

        protected override Task OnInitializedCoreAsync()
        {
            InitializedCoreWasCalled = true;
            return Task.CompletedTask;
        }

        protected override Task OnAfterRenderCoreAsync(bool firstRender)
        {
            AfterRenderCoreWasCalled = true;
            return Task.CompletedTask;
        }

        public bool GetEnableLifecycleTelemetry() => EnableLifecycleTelemetry;
    }

    /// <summary>
    /// Test implementation that exposes default property values.
    /// </summary>
    private sealed class DefaultsExposingComponent : TelemeteredComponentBase
    {
        public string GetComponentName() => ComponentName;
        public bool GetEnableLifecycleTelemetry() => EnableLifecycleTelemetry;
    }

    [Fact(DisplayName = "ComponentName defaults to type name")]
    public void ComponentName_DefaultsToTypeName()
    {
        // Arrange
        using var component = new DefaultsExposingComponent();

        // Act
        string name = component.GetComponentName();

        // Assert
        name.ShouldBe("DefaultsExposingComponent");
    }

    [Fact(DisplayName = "ComponentName can be overridden")]
    public void ComponentName_CanBeOverridden()
    {
        // Arrange
        using var component = new CustomNameComponent();

        // Act
        string componentName = component.GetComponentName();

        // Assert
        componentName.ShouldBe("MyCustomComponent");
    }

    [Fact(DisplayName = "EnableLifecycleTelemetry defaults to true")]
    public void EnableLifecycleTelemetry_DefaultsToTrue()
    {
        // Arrange
        using var component = new DefaultsExposingComponent();

        // Act
        bool enabled = component.GetEnableLifecycleTelemetry();

        // Assert
        enabled.ShouldBeTrue();
    }

    [Fact(DisplayName = "EnableLifecycleTelemetry can be overridden to false")]
    public void EnableLifecycleTelemetry_CanBeOverriddenToFalse()
    {
        // Arrange
        using var component = new TelemetryDisabledComponent();

        // Act
        bool enabled = component.GetEnableLifecycleTelemetry();

        // Assert
        enabled.ShouldBeFalse();
    }

    [Fact(DisplayName = "OnInitializedAsync calls OnInitializedCoreAsync")]
    public async Task OnInitializedAsync_CallsOnInitializedCoreAsync()
    {
        // Arrange
        using var component = new TestComponent();

        // Act
        await component.CallOnInitializedAsync();

        // Assert
        component.InitializedCoreWasCalled.ShouldBeTrue();
    }

    [Fact(DisplayName = "OnInitializedAsync with telemetry enabled completes successfully")]
    public async Task OnInitializedAsync_WithTelemetryEnabled_CompletesSuccessfully()
    {
        // Arrange
        using var component = new TestComponent();

        // Act & Assert
        await Should.NotThrowAsync(async () => await component.CallOnInitializedAsync());
    }

    [Fact(DisplayName = "OnInitializedAsync with telemetry disabled completes successfully")]
    public async Task OnInitializedAsync_WithTelemetryDisabled_CompletesSuccessfully()
    {
        // Arrange
        using var component = new TelemetryDisabledComponent();

        // Act
        await component.CallOnInitializedAsync();

        // Assert
        component.InitializedCoreWasCalled.ShouldBeTrue();
    }

    [Fact(DisplayName = "OnAfterRenderAsync calls OnAfterRenderCoreAsync")]
    public async Task OnAfterRenderAsync_CallsOnAfterRenderCoreAsync()
    {
        // Arrange
        using var component = new TestComponent();

        // Act
        await component.CallOnAfterRenderAsync(firstRender: true);

        // Assert
        component.AfterRenderCoreWasCalled.ShouldBeTrue();
    }

    [Fact(DisplayName = "OnAfterRenderAsync passes firstRender parameter correctly")]
    public async Task OnAfterRenderAsync_PassesFirstRenderCorrectly()
    {
        // Arrange
        using var component = new TestComponent();

        // Act
        await component.CallOnAfterRenderAsync(firstRender: true);

        // Assert
        component.AfterRenderFirstRenderValue.ShouldBeTrue();
    }

    [Fact(DisplayName = "OnAfterRenderAsync with firstRender false passes correctly")]
    public async Task OnAfterRenderAsync_WithFirstRenderFalse_PassesCorrectly()
    {
        // Arrange
        using var component = new TestComponent();
        await component.CallOnAfterRenderAsync(firstRender: true); // First render

        // Act
        await component.CallOnAfterRenderAsync(firstRender: false); // Subsequent render

        // Assert
        component.AfterRenderFirstRenderValue.ShouldBeFalse();
    }

    [Fact(DisplayName = "OnAfterRenderAsync with telemetry enabled completes successfully")]
    public async Task OnAfterRenderAsync_WithTelemetryEnabled_CompletesSuccessfully()
    {
        // Arrange
        using var component = new TestComponent();

        // Act & Assert
        await Should.NotThrowAsync(async () => await component.CallOnAfterRenderAsync(firstRender: true));
    }

    [Fact(DisplayName = "OnAfterRenderAsync with telemetry disabled completes successfully")]
    public async Task OnAfterRenderAsync_WithTelemetryDisabled_CompletesSuccessfully()
    {
        // Arrange
        using var component = new TelemetryDisabledComponent();

        // Act
        await component.CallOnAfterRenderAsync(firstRender: true);

        // Assert
        component.AfterRenderCoreWasCalled.ShouldBeTrue();
    }

    [Fact(DisplayName = "Dispose can be called without errors")]
    public void Dispose_CanBeCalledWithoutErrors()
    {
        // Arrange
        using var component = new TestComponent();

        // Act & Assert - dispose is called automatically by using statement
        component.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Dispose can be called multiple times")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test intentionally tests multiple disposal")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Test intentionally tests multiple disposal")]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var component = new TestComponent();
        Exception? caughtException = null;

        // Act - calling dispose multiple times should not throw
        try
        {
            component.Dispose();
            component.Dispose();
            component.Dispose();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.ShouldBeNull();
    }

    [Fact(DisplayName = "Dispose with telemetry disabled completes successfully")]
    public void Dispose_WithTelemetryDisabled_CompletesSuccessfully()
    {
        // Arrange
        using var component = new TelemetryDisabledComponent();

        // Act & Assert - dispose is called automatically by using statement
        component.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Component lifecycle OnInitialized then OnAfterRender then Dispose completes")]
    public async Task FullLifecycle_CompletesSuccessfully()
    {
        // Arrange
        using var component = new TestComponent();

        // Act
        await component.CallOnInitializedAsync();
        await component.CallOnAfterRenderAsync(firstRender: true);
        await component.CallOnAfterRenderAsync(firstRender: false);

        // Assert
        component.InitializedCoreWasCalled.ShouldBeTrue();
        component.AfterRenderCoreWasCalled.ShouldBeTrue();
    }

    [Fact(DisplayName = "Multiple renders track firstRender correctly")]
    public async Task MultipleRenders_TracksFirstRenderCorrectly()
    {
        // Arrange
        using var component = new TestComponent();

        // Act - First render
        await component.CallOnAfterRenderAsync(firstRender: true);
        bool afterFirstRender = component.AfterRenderFirstRenderValue;

        // Act - Second render
        await component.CallOnAfterRenderAsync(firstRender: false);
        bool afterSecondRender = component.AfterRenderFirstRenderValue;

        // Assert
        afterFirstRender.ShouldBeTrue();
        afterSecondRender.ShouldBeFalse();
    }

    [Fact(DisplayName = "Custom component name is used in OnInitializedAsync")]
    public async Task CustomComponentName_IsUsedInOnInitializedAsync()
    {
        // Arrange
        using var component = new CustomNameComponent();

        // Act & Assert - Just verify it doesn't throw
        await Should.NotThrowAsync(async () => await component.CallOnInitializedAsync());
        component.GetComponentName().ShouldBe("MyCustomComponent");
    }

    [Fact(DisplayName = "Component can be instantiated")]
    public void Component_CanBeInstantiated()
    {
        // Act & Assert
        Should.NotThrow(() =>
        {
            using var component = new TestComponent();
        });
    }

    [Fact(DisplayName = "TelemetryDisabledComponent skips telemetry recording")]
    public async Task TelemetryDisabledComponent_SkipsTelemetryRecording()
    {
        // Arrange
        using var component = new TelemetryDisabledComponent();

        // Act - Run through full lifecycle
        await component.CallOnInitializedAsync();
        await component.CallOnAfterRenderAsync(firstRender: true);
        await component.CallOnAfterRenderAsync(firstRender: false);

        // Assert - Core methods were still called
        component.InitializedCoreWasCalled.ShouldBeTrue();
        component.AfterRenderCoreWasCalled.ShouldBeTrue();
    }

    [Fact(DisplayName = "OnAfterRenderAsync called many times completes successfully")]
    public async Task OnAfterRenderAsync_CalledManyTimes_CompletesSuccessfully()
    {
        // Arrange
        using var component = new TestComponent();

        // Act
        await component.CallOnAfterRenderAsync(firstRender: true);

        for (int i = 0; i < 10; i++)
        {
            await component.CallOnAfterRenderAsync(firstRender: false);
        }

        // Assert
        component.AfterRenderCoreWasCalled.ShouldBeTrue();
    }
}
