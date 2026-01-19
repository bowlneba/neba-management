using Microsoft.AspNetCore.Components.Server.Circuits;
using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Web.Server.Telemetry;

/// <summary>
/// Unit tests for CircuitHealthTelemetry.
/// Note: Tests requiring a Circuit instance are in integration tests since Circuit is sealed
/// and cannot be mocked or stubbed in unit tests.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class CircuitHealthTelemetryTests
{
    [Fact(DisplayName = "OnCircuitOpenedAsync throws ArgumentNullException when circuit is null")]
    public async Task OnCircuitOpenedAsync_WhenCircuitIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await telemetry.OnCircuitOpenedAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "OnCircuitClosedAsync throws ArgumentNullException when circuit is null")]
    public async Task OnCircuitClosedAsync_WhenCircuitIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await telemetry.OnCircuitClosedAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "OnConnectionDownAsync throws ArgumentNullException when circuit is null")]
    public async Task OnConnectionDownAsync_WhenCircuitIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await telemetry.OnConnectionDownAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "OnConnectionUpAsync throws ArgumentNullException when circuit is null")]
    public async Task OnConnectionUpAsync_WhenCircuitIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await telemetry.OnConnectionUpAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "CircuitHealthTelemetry can be instantiated")]
    public void CircuitHealthTelemetry_CanBeInstantiated()
    {
        // Act & Assert
        Should.NotThrow(() => new CircuitHealthTelemetry());
    }
}
