using System.Diagnostics;
using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Web.Server.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class LoadingStateTelemetryTests
{
    [Fact(DisplayName = "RecordDataLoad with success true records successfully")]
    public void RecordDataLoad_WithSuccessTrue_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "LoadUsers";
        double durationMs = 125.5;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: true));
    }

    [Fact(DisplayName = "RecordDataLoad with success false records successfully")]
    public void RecordDataLoad_WithSuccessFalse_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "LoadUsers";
        double durationMs = 250.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: false));
    }

    [Fact(DisplayName = "RecordDataLoad with item count records successfully")]
    public void RecordDataLoad_WithItemCount_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "LoadUsers";
        double durationMs = 100.0;
        int itemCount = 50;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: true, itemCount));
    }

    [Fact(DisplayName = "RecordDataLoad without item count records successfully")]
    public void RecordDataLoad_WithoutItemCount_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "LoadSettings";
        double durationMs = 50.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: true, null));
    }

    [Fact(DisplayName = "RecordDataLoad with zero duration records successfully")]
    public void RecordDataLoad_WithZeroDuration_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "CachedData";
        double durationMs = 0.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: true));
    }

    [Fact(DisplayName = "RecordDataLoad with large duration records successfully")]
    public void RecordDataLoad_WithLargeDuration_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "SlowQuery";
        double durationMs = 5000.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: true));
    }

    [Fact(DisplayName = "RecordDataLoad with zero item count records successfully")]
    public void RecordDataLoad_WithZeroItemCount_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "EmptyResult";
        double durationMs = 50.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: true, itemCount: 0));
    }

    [Fact(DisplayName = "RecordDataLoad with large item count records successfully")]
    public void RecordDataLoad_WithLargeItemCount_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "BulkLoad";
        double durationMs = 1000.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordDataLoad(operationName, durationMs, success: true, itemCount: 10000));
    }

    [Fact(DisplayName = "RecordTimeToFirstRender records successfully")]
    public void RecordTimeToFirstRender_RecordsSuccessfully()
    {
        // Arrange
        string componentName = "UserDashboard";
        double durationMs = 75.5;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordTimeToFirstRender(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordTimeToFirstRender with zero duration records successfully")]
    public void RecordTimeToFirstRender_WithZeroDuration_RecordsSuccessfully()
    {
        // Arrange
        string componentName = "SimpleComponent";
        double durationMs = 0.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordTimeToFirstRender(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordTimeToFirstRender with large duration records successfully")]
    public void RecordTimeToFirstRender_WithLargeDuration_RecordsSuccessfully()
    {
        // Arrange
        string componentName = "ComplexDashboard";
        double durationMs = 3000.0;

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordTimeToFirstRender(componentName, durationMs));
    }

    [Fact(DisplayName = "RecordLoadingState with loading state records successfully")]
    public void RecordLoadingState_WithLoadingState_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "FetchData";
        string state = "loading";

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordLoadingState(operationName, state));
    }

    [Fact(DisplayName = "RecordLoadingState with loaded state records successfully")]
    public void RecordLoadingState_WithLoadedState_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "FetchData";
        string state = "loaded";

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordLoadingState(operationName, state));
    }

    [Fact(DisplayName = "RecordLoadingState with error state records successfully")]
    public void RecordLoadingState_WithErrorState_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "FetchData";
        string state = "error";

        // Act & Assert
        Should.NotThrow(() => LoadingStateTelemetry.RecordLoadingState(operationName, state));
    }

    [Fact(DisplayName = "StartLoadingTimer returns running stopwatch")]
    public void StartLoadingTimer_ReturnsRunningStopwatch()
    {
        // Arrange
        string operationName = "LoadData";

        // Act
        Stopwatch stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);

        // Assert
        stopwatch.ShouldNotBeNull();
        stopwatch.IsRunning.ShouldBeTrue();
    }

    [Fact(DisplayName = "StartLoadingTimer records loading state")]
    public void StartLoadingTimer_RecordsLoadingState()
    {
        // Arrange
        string operationName = "LoadUsers";

        // Act & Assert
        Should.NotThrow(() =>
        {
            Stopwatch stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);
            stopwatch.ShouldNotBeNull();
        });
    }

    [Fact(DisplayName = "StopLoadingTimer stops stopwatch and records data")]
    public void StopLoadingTimer_StopsStopwatchAndRecordsData()
    {
        // Arrange
        string operationName = "LoadData";
        Stopwatch stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);

        // Act
        LoadingStateTelemetry.StopLoadingTimer(operationName, stopwatch, success: true);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
    }

    [Fact(DisplayName = "StopLoadingTimer with item count records successfully")]
    public void StopLoadingTimer_WithItemCount_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "LoadUsers";
        Stopwatch stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);

        // Act
        LoadingStateTelemetry.StopLoadingTimer(operationName, stopwatch, success: true, itemCount: 25);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact(DisplayName = "StopLoadingTimer with success false records successfully")]
    public void StopLoadingTimer_WithSuccessFalse_RecordsSuccessfully()
    {
        // Arrange
        string operationName = "FailedLoad";
        Stopwatch stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);

        // Act
        LoadingStateTelemetry.StopLoadingTimer(operationName, stopwatch, success: false);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
    }

    [Fact(DisplayName = "StopLoadingTimer throws ArgumentNullException when stopwatch is null")]
    public void StopLoadingTimer_WhenStopwatchIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string operationName = "LoadData";

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            LoadingStateTelemetry.StopLoadingTimer(operationName, null!, success: true));
    }

    [Fact(DisplayName = "Complete loading cycle executes successfully")]
    public async Task CompleteLoadingCycle_ExecutesSuccessfully()
    {
        // Arrange
        string operationName = "LoadDashboard";

        // Act
        Stopwatch stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);
        stopwatch.IsRunning.ShouldBeTrue();

        await Task.Delay(10); // Simulate work

        LoadingStateTelemetry.StopLoadingTimer(operationName, stopwatch, success: true, itemCount: 10);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Multiple concurrent loading operations can be tracked")]
    public async Task MultipleConcurrentLoadingOperations_CanBeTracked()
    {
        // Arrange & Act
        var sw1 = LoadingStateTelemetry.StartLoadingTimer("Operation1");
        await Task.Delay(10);

        var sw2 = LoadingStateTelemetry.StartLoadingTimer("Operation2");
        await Task.Delay(10);

        var sw3 = LoadingStateTelemetry.StartLoadingTimer("Operation3");
        await Task.Delay(10);

        // Stop in different order
        LoadingStateTelemetry.StopLoadingTimer("Operation2", sw2, success: true, itemCount: 5);
        await Task.Delay(10);

        LoadingStateTelemetry.StopLoadingTimer("Operation1", sw1, success: true, itemCount: 3);
        await Task.Delay(10);

        LoadingStateTelemetry.StopLoadingTimer("Operation3", sw3, success: false);

        // Assert
        sw1.IsRunning.ShouldBeFalse();
        sw2.IsRunning.ShouldBeFalse();
        sw3.IsRunning.ShouldBeFalse();
    }

    [Fact(DisplayName = "Loading state transitions can be tracked manually")]
    public void LoadingStateTransitions_CanBeTrackedManually()
    {
        // Arrange
        string operationName = "ManualOperation";

        // Act & Assert
        Should.NotThrow(() =>
        {
            LoadingStateTelemetry.RecordLoadingState(operationName, "loading");
            LoadingStateTelemetry.RecordLoadingState(operationName, "loaded");
        });
    }

    [Fact(DisplayName = "Error state transition can be tracked")]
    public void ErrorStateTransition_CanBeTracked()
    {
        // Arrange
        string operationName = "ErrorOperation";

        // Act & Assert
        Should.NotThrow(() =>
        {
            LoadingStateTelemetry.RecordLoadingState(operationName, "loading");
            LoadingStateTelemetry.RecordLoadingState(operationName, "error");
        });
    }

    [Fact(DisplayName = "Multiple operations with same name can be tracked")]
    public void MultipleOperations_WithSameName_CanBeTracked()
    {
        // Arrange
        string operationName = "RepeatedOperation";

        // Act & Assert
        Should.NotThrow(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                var sw = LoadingStateTelemetry.StartLoadingTimer(operationName);
                LoadingStateTelemetry.StopLoadingTimer(operationName, sw, success: true, itemCount: i);
            }
        });
    }

    [Fact(DisplayName = "Very quick operation can be measured")]
    public void VeryQuickOperation_CanBeMeasured()
    {
        // Arrange
        string operationName = "QuickOperation";

        // Act
        var stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);
        LoadingStateTelemetry.StopLoadingTimer(operationName, stopwatch, success: true);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
        stopwatch.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact(DisplayName = "Long running operation can be measured")]
    public async Task LongRunningOperation_CanBeMeasured()
    {
        // Arrange
        string operationName = "LongOperation";
        int delayMs = 100;

        // Act
        var stopwatch = LoadingStateTelemetry.StartLoadingTimer(operationName);
        await Task.Delay(delayMs);
        LoadingStateTelemetry.StopLoadingTimer(operationName, stopwatch, success: true);

        // Assert
        stopwatch.IsRunning.ShouldBeFalse();
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(delayMs - 20); // Allow for timing variance
    }
}
