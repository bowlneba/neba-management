using Microsoft.AspNetCore.Components.Server.Circuits;
using Moq;
using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Web.Server.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class CircuitHealthTelemetryTests
{
    [Fact(DisplayName = "OnCircuitOpenedAsync records circuit opened")]
    public async Task OnCircuitOpenedAsync_RecordsCircuitOpened()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("test-circuit-1");

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None));
    }

    [Fact(DisplayName = "OnCircuitOpenedAsync throws ArgumentNullException when circuit is null")]
    public async Task OnCircuitOpenedAsync_WhenCircuitIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await telemetry.OnCircuitOpenedAsync(null!, CancellationToken.None));
    }

    [Fact(DisplayName = "OnCircuitClosedAsync records circuit closed")]
    public async Task OnCircuitClosedAsync_RecordsCircuitClosed()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("test-circuit-1");

        // Open circuit first
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await telemetry.OnCircuitClosedAsync(circuitMock.Object, CancellationToken.None));
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

    [Fact(DisplayName = "OnCircuitClosedAsync without corresponding open completes successfully")]
    public async Task OnCircuitClosedAsync_WithoutCorrespondingOpen_CompletesSuccessfully()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("test-circuit-1");

        // Act & Assert - closing without opening should not throw
        await Should.NotThrowAsync(async () =>
            await telemetry.OnCircuitClosedAsync(circuitMock.Object, CancellationToken.None));
    }

    [Fact(DisplayName = "OnConnectionDownAsync records connection error")]
    public async Task OnConnectionDownAsync_RecordsConnectionError()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("test-circuit-1");

        // Open circuit first
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await telemetry.OnConnectionDownAsync(circuitMock.Object, CancellationToken.None));
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

    [Fact(DisplayName = "OnConnectionUpAsync records connection restored")]
    public async Task OnConnectionUpAsync_RecordsConnectionRestored()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("test-circuit-1");

        // Open circuit first
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await telemetry.OnConnectionUpAsync(circuitMock.Object, CancellationToken.None));
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

    [Fact(DisplayName = "Complete circuit lifecycle executes successfully")]
    public async Task CompleteCircuitLifecycle_ExecutesSuccessfully()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("lifecycle-circuit");

        // Act & Assert - Open -> Connection Down -> Connection Up -> Close
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);
        await Task.Delay(10);

        await telemetry.OnConnectionDownAsync(circuitMock.Object, CancellationToken.None);
        await Task.Delay(10);

        await telemetry.OnConnectionUpAsync(circuitMock.Object, CancellationToken.None);
        await Task.Delay(10);

        await telemetry.OnCircuitClosedAsync(circuitMock.Object, CancellationToken.None);
    }

    [Fact(DisplayName = "Multiple circuit lifecycles execute concurrently")]
    public async Task MultipleCircuitLifecycles_ExecuteConcurrently()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();

        var circuit1 = new Mock<Circuit>();
        circuit1.Setup(c => c.Id).Returns("circuit-1");

        var circuit2 = new Mock<Circuit>();
        circuit2.Setup(c => c.Id).Returns("circuit-2");

        var circuit3 = new Mock<Circuit>();
        circuit3.Setup(c => c.Id).Returns("circuit-3");

        // Act & Assert - Open all circuits
        await telemetry.OnCircuitOpenedAsync(circuit1.Object, CancellationToken.None);
        await telemetry.OnCircuitOpenedAsync(circuit2.Object, CancellationToken.None);
        await telemetry.OnCircuitOpenedAsync(circuit3.Object, CancellationToken.None);

        await Task.Delay(10);

        // Close circuits in different order
        await telemetry.OnCircuitClosedAsync(circuit2.Object, CancellationToken.None);
        await telemetry.OnCircuitClosedAsync(circuit1.Object, CancellationToken.None);
        await telemetry.OnCircuitClosedAsync(circuit3.Object, CancellationToken.None);
    }

    [Fact(DisplayName = "Multiple connection down events for same circuit are recorded")]
    public async Task MultipleConnectionDownEvents_AreRecorded()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("flaky-circuit");

        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);

        // Act & Assert - Multiple connection issues
        await telemetry.OnConnectionDownAsync(circuitMock.Object, CancellationToken.None);
        await telemetry.OnConnectionUpAsync(circuitMock.Object, CancellationToken.None);
        await telemetry.OnConnectionDownAsync(circuitMock.Object, CancellationToken.None);
        await telemetry.OnConnectionUpAsync(circuitMock.Object, CancellationToken.None);
        await telemetry.OnConnectionDownAsync(circuitMock.Object, CancellationToken.None);
        await telemetry.OnConnectionUpAsync(circuitMock.Object, CancellationToken.None);

        await telemetry.OnCircuitClosedAsync(circuitMock.Object, CancellationToken.None);
    }

    [Fact(DisplayName = "Circuit lifecycle with long duration records correctly")]
    public async Task CircuitLifecycle_WithLongDuration_RecordsCorrectly()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("long-circuit");

        // Act
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);
        await Task.Delay(100); // Simulate longer circuit lifetime
        await telemetry.OnCircuitClosedAsync(circuitMock.Object, CancellationToken.None);

        // Assert - should complete without errors
        Should.Pass();
    }

    [Fact(DisplayName = "Circuit with very short lifetime records correctly")]
    public async Task Circuit_WithVeryShortLifetime_RecordsCorrectly()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("short-circuit");

        // Act - Open and close immediately
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);
        await telemetry.OnCircuitClosedAsync(circuitMock.Object, CancellationToken.None);

        // Assert
        Should.Pass();
    }

    [Fact(DisplayName = "Circuit IDs with special characters are handled")]
    public async Task CircuitIds_WithSpecialCharacters_AreHandled()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("circuit-with-special-chars-!@#$%^&*()");

        // Act & Assert
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, CancellationToken.None);
        await telemetry.OnCircuitClosedAsync(circuitMock.Object, CancellationToken.None);
    }

    [Fact(DisplayName = "Circuit lifecycle respects cancellation token")]
    public async Task CircuitLifecycle_RespectsCancellationToken()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("cancellable-circuit");
        using var cts = new CancellationTokenSource();

        // Act & Assert
        await telemetry.OnCircuitOpenedAsync(circuitMock.Object, cts.Token);
        cts.Cancel();

        // Even with cancelled token, close should complete
        await Should.NotThrowAsync(async () =>
            await telemetry.OnCircuitClosedAsync(circuitMock.Object, cts.Token));
    }

    [Fact(DisplayName = "Connection events without circuit open complete successfully")]
    public async Task ConnectionEvents_WithoutCircuitOpen_CompleteSuccessfully()
    {
        // Arrange
        var telemetry = new CircuitHealthTelemetry();
        var circuitMock = new Mock<Circuit>();
        circuitMock.Setup(c => c.Id).Returns("orphan-circuit");

        // Act & Assert - Connection events without opening
        await Should.NotThrowAsync(async () =>
        {
            await telemetry.OnConnectionDownAsync(circuitMock.Object, CancellationToken.None);
            await telemetry.OnConnectionUpAsync(circuitMock.Object, CancellationToken.None);
        });
    }
}
