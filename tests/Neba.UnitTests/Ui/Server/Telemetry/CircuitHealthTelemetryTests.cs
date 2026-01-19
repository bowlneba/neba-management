using Microsoft.AspNetCore.Components.Server.Circuits;
using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Ui.Server.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class CircuitHealthTelemetryTests
{
    private readonly CircuitHealthTelemetry _telemetry = new();

    [Fact(DisplayName = "OnCircuitOpenedAsync throws ArgumentNullException for null circuit")]
    public async Task OnCircuitOpenedAsync_NullCircuit_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            _telemetry.OnCircuitOpenedAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "OnCircuitClosedAsync throws ArgumentNullException for null circuit")]
    public async Task OnCircuitClosedAsync_NullCircuit_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            _telemetry.OnCircuitClosedAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "OnConnectionDownAsync throws ArgumentNullException for null circuit")]
    public async Task OnConnectionDownAsync_NullCircuit_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            _telemetry.OnConnectionDownAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "OnConnectionUpAsync throws ArgumentNullException for null circuit")]
    public async Task OnConnectionUpAsync_NullCircuit_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            _telemetry.OnConnectionUpAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "CircuitHealthTelemetry inherits from CircuitHandler")]
    public void CircuitHealthTelemetry_InheritsFromCircuitHandler()
    {
        // Assert
        _telemetry.ShouldBeAssignableTo<CircuitHandler>();
    }

    [Fact(DisplayName = "CircuitHealthTelemetry can be instantiated")]
    public void CircuitHealthTelemetry_CanBeInstantiated()
    {
        // Act
        var telemetry = new CircuitHealthTelemetry();

        // Assert
        telemetry.ShouldNotBeNull();
    }
}
