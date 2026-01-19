using Neba.Infrastructure.BackgroundJobs;

namespace Neba.UnitTests.Infrastructure.BackgroundJobs;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.BackgroundJobs")]
public sealed class HangfireMetricsTests
{
    [Fact(DisplayName = "RecordJobStart with valid job type completes successfully")]
    public void RecordJobStart_WithValidJobType_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "DocumentRefreshJob";

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobStart(jobType));
    }

    [Fact(DisplayName = "RecordJobStart with different job types completes successfully")]
    public void RecordJobStart_WithDifferentJobTypes_CompletesSuccessfully()
    {
        // Arrange & Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobStart("DocumentRefreshJob"));
        Should.NotThrow(() => HangfireMetrics.RecordJobStart("ExportJob"));
        Should.NotThrow(() => HangfireMetrics.RecordJobStart("MaintenanceJob"));
    }

    [Fact(DisplayName = "RecordJobSuccess with valid parameters completes successfully")]
    public void RecordJobSuccess_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "DocumentRefreshJob";
        double durationMs = 1234.5;

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobSuccess(jobType, durationMs));
    }

    [Fact(DisplayName = "RecordJobSuccess with fast execution completes successfully")]
    public void RecordJobSuccess_WithFastExecution_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "QuickJob";
        double durationMs = 50.0;

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobSuccess(jobType, durationMs));
    }

    [Fact(DisplayName = "RecordJobSuccess with slow execution completes successfully")]
    public void RecordJobSuccess_WithSlowExecution_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "SlowJob";
        double durationMs = 30000.0;

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobSuccess(jobType, durationMs));
    }

    [Fact(DisplayName = "RecordJobSuccess with zero duration completes successfully")]
    public void RecordJobSuccess_WithZeroDuration_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "InstantJob";
        double durationMs = 0.0;

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobSuccess(jobType, durationMs));
    }

    [Fact(DisplayName = "RecordJobFailure with valid parameters completes successfully")]
    public void RecordJobFailure_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "FailingJob";
        double durationMs = 5000.0;
        string errorType = "InvalidOperationException";

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobFailure(jobType, durationMs, errorType));
    }

    [Fact(DisplayName = "RecordJobFailure with different error types completes successfully")]
    public void RecordJobFailure_WithDifferentErrorTypes_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "FailingJob";
        double durationMs = 2500.0;

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobFailure(jobType, durationMs, "TimeoutException"));
        Should.NotThrow(() => HangfireMetrics.RecordJobFailure(jobType, durationMs, "HttpRequestException"));
        Should.NotThrow(() => HangfireMetrics.RecordJobFailure(jobType, durationMs, "JsonException"));
    }

    [Fact(DisplayName = "RecordJobFailure with quick failure completes successfully")]
    public void RecordJobFailure_WithQuickFailure_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "QuickFailJob";
        double durationMs = 10.0;
        string errorType = "NullReferenceException";

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobFailure(jobType, durationMs, errorType));
    }

    [Fact(DisplayName = "RecordJobStart and RecordJobSuccess sequence completes successfully")]
    public void RecordJobSequence_StartThenSuccess_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "SequencedJob";
        double durationMs = 1500.0;

        // Act & Assert
        Should.NotThrow(() =>
        {
            HangfireMetrics.RecordJobStart(jobType);
            HangfireMetrics.RecordJobSuccess(jobType, durationMs);
        });
    }

    [Fact(DisplayName = "RecordJobStart and RecordJobFailure sequence completes successfully")]
    public void RecordJobSequence_StartThenFailure_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "FailingSequencedJob";
        double durationMs = 3000.0;
        string errorType = "OperationCanceledException";

        // Act & Assert
        Should.NotThrow(() =>
        {
            HangfireMetrics.RecordJobStart(jobType);
            HangfireMetrics.RecordJobFailure(jobType, durationMs, errorType);
        });
    }

    [Fact(DisplayName = "Multiple concurrent job recordings complete successfully")]
    public void MultipleConcurrentJobRecordings_CompleteSuccessfully()
    {
        // Act & Assert
        Should.NotThrow(() =>
        {
            HangfireMetrics.RecordJobStart("Job1");
            HangfireMetrics.RecordJobStart("Job2");
            HangfireMetrics.RecordJobSuccess("Job1", 1000.0);
            HangfireMetrics.RecordJobFailure("Job2", 2000.0, "Exception");
            HangfireMetrics.RecordJobStart("Job3");
            HangfireMetrics.RecordJobSuccess("Job3", 1500.0);
        });
    }

    [Fact(DisplayName = "RecordJobSuccess with decimal duration completes successfully")]
    public void RecordJobSuccess_WithDecimalDuration_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "PrecisionJob";
        double durationMs = 1234.5678;

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobSuccess(jobType, durationMs));
    }

    [Fact(DisplayName = "RecordJobFailure with special characters in error type completes successfully")]
    public void RecordJobFailure_WithSpecialCharactersInErrorType_CompletesSuccessfully()
    {
        // Arrange
        string jobType = "SpecialJob";
        double durationMs = 1000.0;
        string errorType = "Namespace.Nested+InnerException";

        // Act & Assert
        Should.NotThrow(() => HangfireMetrics.RecordJobFailure(jobType, durationMs, errorType));
    }
}
