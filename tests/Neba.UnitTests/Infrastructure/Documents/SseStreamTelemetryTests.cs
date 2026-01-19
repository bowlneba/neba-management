using System.Diagnostics;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.Documents;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Documents")]
public sealed class SseStreamTelemetryTests
{
    [Fact(DisplayName = "RecordConnectionStart returns running stopwatch")]
    public void RecordConnectionStart_ReturnsRunningStopwatch()
    {
        // Arrange
        string streamType = "document";

        // Act
        Stopwatch stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);

        // Assert
        stopwatch.ShouldNotBeNull();
        stopwatch.IsRunning.ShouldBeTrue();
    }

    [Fact(DisplayName = "RecordConnectionStart with different stream types returns stopwatch")]
    public void RecordConnectionStart_WithDifferentStreamTypes_ReturnsStopwatch()
    {
        // Arrange & Act & Assert
        var sw1 = SseStreamTelemetry.RecordConnectionStart("document");
        sw1.ShouldNotBeNull();
        sw1.IsRunning.ShouldBeTrue();

        var sw2 = SseStreamTelemetry.RecordConnectionStart("notification");
        sw2.ShouldNotBeNull();
        sw2.IsRunning.ShouldBeTrue();

        var sw3 = SseStreamTelemetry.RecordConnectionStart("status");
        sw3.ShouldNotBeNull();
        sw3.IsRunning.ShouldBeTrue();
    }

    [Fact(DisplayName = "RecordConnectionEnd stops the stopwatch")]
    public void RecordConnectionEnd_StopsTheStopwatch()
    {
        // Arrange
        string streamType = "document";
        var stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);
        int eventCount = 5;

        // Act
        SseStreamTelemetry.RecordConnectionEnd(streamType, stopwatch, eventCount);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
    }

    [Fact(DisplayName = "RecordConnectionEnd with zero events completes successfully")]
    public void RecordConnectionEnd_WithZeroEvents_CompletesSuccessfully()
    {
        // Arrange
        string streamType = "document";
        var stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);

        // Act & Assert
        Should.NotThrow(() => SseStreamTelemetry.RecordConnectionEnd(streamType, stopwatch, 0));
        stopwatch.IsRunning.ShouldBeFalse();
    }

    [Fact(DisplayName = "RecordConnectionEnd with multiple events completes successfully")]
    public void RecordConnectionEnd_WithMultipleEvents_CompletesSuccessfully()
    {
        // Arrange
        string streamType = "notification";
        var stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);

        // Act & Assert
        Should.NotThrow(() => SseStreamTelemetry.RecordConnectionEnd(streamType, stopwatch, 100));
        stopwatch.IsRunning.ShouldBeFalse();
    }

    [Fact(DisplayName = "RecordEventPublished with valid parameters completes successfully")]
    public void RecordEventPublished_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        string streamType = "document";
        string eventType = "content.updated";

        // Act & Assert
        Should.NotThrow(() => SseStreamTelemetry.RecordEventPublished(streamType, eventType));
    }

    [Fact(DisplayName = "RecordEventPublished with different event types completes successfully")]
    public void RecordEventPublished_WithDifferentEventTypes_CompletesSuccessfully()
    {
        // Arrange
        string streamType = "document";

        // Act & Assert
        Should.NotThrow(() => SseStreamTelemetry.RecordEventPublished(streamType, "content.created"));
        Should.NotThrow(() => SseStreamTelemetry.RecordEventPublished(streamType, "content.updated"));
        Should.NotThrow(() => SseStreamTelemetry.RecordEventPublished(streamType, "content.deleted"));
    }

    [Fact(DisplayName = "RecordConnectionError with valid parameters completes successfully")]
    public void RecordConnectionError_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        string streamType = "document";
        string errorType = "System.OperationCanceledException";

        // Act & Assert
        Should.NotThrow(() => SseStreamTelemetry.RecordConnectionError(streamType, errorType));
    }

    [Fact(DisplayName = "RecordConnectionError with different error types completes successfully")]
    public void RecordConnectionError_WithDifferentErrorTypes_CompletesSuccessfully()
    {
        // Arrange
        string streamType = "notification";

        // Act & Assert
        Should.NotThrow(() => SseStreamTelemetry.RecordConnectionError(streamType, "System.InvalidOperationException"));
        Should.NotThrow(() => SseStreamTelemetry.RecordConnectionError(streamType, "System.TimeoutException"));
        Should.NotThrow(() => SseStreamTelemetry.RecordConnectionError(streamType, "System.IO.IOException"));
    }

    [Fact(DisplayName = "Complete connection lifecycle executes successfully")]
    public async Task CompleteConnectionLifecycle_ExecutesSuccessfully()
    {
        // Arrange
        string streamType = "document";

        // Act - Start connection
        var stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);
        stopwatch.IsRunning.ShouldBeTrue();

        // Simulate some events
        await Task.Delay(10); // Small delay to simulate actual work
        SseStreamTelemetry.RecordEventPublished(streamType, "content.updated");
        SseStreamTelemetry.RecordEventPublished(streamType, "content.created");

        await Task.Delay(10);

        // End connection
        SseStreamTelemetry.RecordConnectionEnd(streamType, stopwatch, 2);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Connection error lifecycle executes successfully")]
    public async Task ConnectionErrorLifecycle_ExecutesSuccessfully()
    {
        // Arrange
        string streamType = "notification";

        // Act - Start connection
        var stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);
        stopwatch.IsRunning.ShouldBeTrue();

        // Simulate some events before error
        await Task.Delay(10);
        SseStreamTelemetry.RecordEventPublished(streamType, "notification.sent");

        // Record error
        SseStreamTelemetry.RecordConnectionError(streamType, "System.OperationCanceledException");

        // Assert
        stopwatch.IsRunning.ShouldBeTrue(); // Error doesn't auto-stop the stopwatch
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Multiple concurrent connections can be tracked independently")]
    public async Task MultipleConcurrentConnections_TrackedIndependently()
    {
        // Arrange & Act
        var sw1 = SseStreamTelemetry.RecordConnectionStart("document");
        await Task.Delay(10);

        var sw2 = SseStreamTelemetry.RecordConnectionStart("notification");
        await Task.Delay(10);

        var sw3 = SseStreamTelemetry.RecordConnectionStart("status");
        await Task.Delay(10);

        // End connections in different order
        SseStreamTelemetry.RecordConnectionEnd("notification", sw2, 5);
        await Task.Delay(10);

        SseStreamTelemetry.RecordConnectionEnd("document", sw1, 3);
        await Task.Delay(10);

        SseStreamTelemetry.RecordConnectionEnd("status", sw3, 10);

        // Assert
        sw1.IsRunning.ShouldBeFalse();
        sw2.IsRunning.ShouldBeFalse();
        sw3.IsRunning.ShouldBeFalse();

        // sw1 should have the longest elapsed time (started first, ended second)
        sw1.ElapsedMilliseconds.ShouldBeGreaterThan(sw2.ElapsedMilliseconds);
        sw3.ElapsedMilliseconds.ShouldBeLessThan(sw1.ElapsedMilliseconds);
    }

    [Fact(DisplayName = "RecordEventPublished can be called multiple times for same stream")]
    public void RecordEventPublished_CalledMultipleTimes_CompletesSuccessfully()
    {
        // Arrange
        string streamType = "document";

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            Should.NotThrow(() => SseStreamTelemetry.RecordEventPublished(streamType, $"event.type.{i}"));
        }
    }

    [Fact(DisplayName = "Stopwatch can measure very short connections")]
    public void StopwatchMeasuresShortConnections()
    {
        // Arrange
        string streamType = "status";

        // Act
        var stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);
        SseStreamTelemetry.RecordConnectionEnd(streamType, stopwatch, 0);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
        stopwatch.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact(DisplayName = "Stopwatch can measure longer connections")]
    public async Task StopwatchMeasuresLongerConnections()
    {
        // Arrange
        string streamType = "document";
        int delayMs = 100;

        // Act
        var stopwatch = SseStreamTelemetry.RecordConnectionStart(streamType);
        await Task.Delay(delayMs);
        SseStreamTelemetry.RecordConnectionEnd(streamType, stopwatch, 0);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(delayMs - 20); // Allow for timing variance
    }
}
