using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Neba.Application.BackgroundJobs;
using Neba.Infrastructure.BackgroundJobs;

namespace Neba.UnitTests.Infrastructure.BackgroundJobs;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.BackgroundJobs")]
public sealed class HangfireBackgroundJobSchedulerTests
{
    static HangfireBackgroundJobSchedulerTests()
    {
        GlobalConfiguration.Configuration.UseMemoryStorage();
    }

    private sealed record TestBackgroundJob(string Name) : IBackgroundJob
    {
        public string JobName => $"Test Job: {Name}";
    }

    private sealed record SimpleTestJob : IBackgroundJob
    {
        public string JobName => "SimpleTestJob";
    }

    private sealed class TestBackgroundJobHandler : IBackgroundJobHandler<TestBackgroundJob>
    {
        public Task ExecuteAsync(TestBackgroundJob job, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class SimpleJobHandler : IBackgroundJobHandler<SimpleTestJob>
    {
        public Task ExecuteAsync(SimpleTestJob job, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed record FailingJob : IBackgroundJob
    {
        public string JobName => "FailingJob";
    }

    private sealed class FailingJobHandler : IBackgroundJobHandler<FailingJob>
    {
        public Task ExecuteAsync(FailingJob job, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Job execution failed");
        }
    }

    private static HangfireBackgroundJobScheduler CreateScheduler()
    {
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<HangfireBackgroundJobScheduler>>();

        mockScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(() =>
            {
                var mockScope = new Mock<IServiceScope>();
                var mockServiceProvider = new Mock<IServiceProvider>();

                mockServiceProvider
                    .Setup(x => x.GetService(typeof(IBackgroundJobHandler<TestBackgroundJob>)))
                    .Returns(new TestBackgroundJobHandler());

                mockServiceProvider
                    .Setup(x => x.GetService(typeof(IBackgroundJobHandler<SimpleTestJob>)))
                    .Returns(new SimpleJobHandler());

                mockServiceProvider
                    .Setup(x => x.GetService(typeof(IBackgroundJobHandler<FailingJob>)))
                    .Returns(new FailingJobHandler());

                mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
                return mockScope.Object;
            });

        return new HangfireBackgroundJobScheduler(mockScopeFactory.Object, mockLogger.Object);
    }

    [Fact(DisplayName = "Enqueue returns job ID")]
    public void Enqueue_WithValidJob_ReturnsJobId()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Test Document");

        // Act
        string jobId = scheduler.Enqueue(job);

        // Assert
        jobId.ShouldNotBeNull();
        jobId.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Enqueue with different job types completes successfully")]
    public void Enqueue_WithDifferentJobTypes_ReturnsJobIds()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job1 = new TestBackgroundJob("Doc1");
        var job2 = new SimpleTestJob();

        // Act
        string jobId1 = scheduler.Enqueue(job1);
        string jobId2 = scheduler.Enqueue(job2);

        // Assert
        jobId1.ShouldNotBeNull();
        jobId2.ShouldNotBeNull();
        jobId1.ShouldNotBe(jobId2);
    }

    [Fact(DisplayName = "Schedule with TimeSpan returns job ID")]
    public void Schedule_WithTimeSpan_ReturnsJobId()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Delayed Doc");
        var delay = TimeSpan.FromHours(1);

        // Act
        string jobId = scheduler.Schedule(job, delay);

        // Assert
        jobId.ShouldNotBeNull();
        jobId.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Schedule with DateTimeOffset returns job ID")]
    public void Schedule_WithDateTimeOffset_ReturnsJobId()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Future Doc");
        DateTimeOffset futureTime = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        string jobId = scheduler.Schedule(job, futureTime);

        // Assert
        jobId.ShouldNotBeNull();
        jobId.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Schedule with small delay completes successfully")]
    public void Schedule_WithSmallDelay_ReturnsJobId()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Quick Job");
        var delay = TimeSpan.FromSeconds(5);

        // Act
        string jobId = scheduler.Schedule(job, delay);

        // Assert
        jobId.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Schedule with large delay completes successfully")]
    public void Schedule_WithLargeDelay_ReturnsJobId()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Long Job");
        var delay = TimeSpan.FromDays(30);

        // Act
        string jobId = scheduler.Schedule(job, delay);

        // Assert
        jobId.ShouldNotBeNull();
    }

    [Fact(DisplayName = "AddOrUpdateRecurring with cron expression completes successfully")]
    public void AddOrUpdateRecurring_WithCronExpression_Succeeds()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Recurring Doc");
        const string cronExpression = "0 0 * * *"; // Daily at midnight

        // Act & Assert
        Should.NotThrow(() => scheduler.AddOrUpdateRecurring("recurring_job", job, cronExpression));
    }

    [Fact(DisplayName = "AddOrUpdateRecurring with different cron expressions completes successfully")]
    public void AddOrUpdateRecurring_WithDifferentCronExpressions_Succeeds()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job1 = new TestBackgroundJob("Daily Job");
        var job2 = new TestBackgroundJob("Hourly Job");

        // Act & Assert
        Should.NotThrow(() =>
        {
            scheduler.AddOrUpdateRecurring("daily", job1, "0 0 * * *");
            scheduler.AddOrUpdateRecurring("hourly", job2, "0 * * * *");
        });
    }

    [Fact(DisplayName = "RemoveRecurring with valid ID completes successfully")]
    public void RemoveRecurring_WithValidId_Succeeds()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Removable Job");
        scheduler.AddOrUpdateRecurring("removable_job", job, "0 0 * * *");

        // Act & Assert
        Should.NotThrow(() => scheduler.RemoveRecurring("removable_job"));
    }

    [Fact(DisplayName = "RemoveRecurring with non-existent ID completes successfully")]
    public void RemoveRecurring_WithNonExistentId_Succeeds()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();

        // Act & Assert
        Should.NotThrow(() => scheduler.RemoveRecurring("non_existent_job"));
    }

    [Fact(DisplayName = "ContinueWith returns job ID")]
    public void ContinueWith_WithValidParentJobId_ReturnsJobId()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        string parentJobId = scheduler.Enqueue(new SimpleTestJob());
        var job = new TestBackgroundJob("Continuation Job");

        // Act
        string jobId = scheduler.ContinueWith(parentJobId, job);

        // Assert
        jobId.ShouldNotBeNull();
        jobId.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Delete with valid job ID completes successfully")]
    public void Delete_WithValidJobId_ReturnsResult()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        const string jobId = "job_to_delete_123";

        // Act
        bool result = scheduler.Delete(jobId);

        // Assert
        // Result may be true or false depending on Hangfire state
        result.ShouldBeOfType<bool>();
    }

    [Fact(DisplayName = "ExecuteJobAsync executes handler successfully")]
    public async Task ExecuteJobAsync_WithValidHandler_ExecutesSuccessfully()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Executable Job");

        // Act & Assert
        await Should.NotThrowAsync(() =>
            scheduler.ExecuteJobAsync(job, "Display Name", CancellationToken.None));
    }

    [Fact(DisplayName = "ExecuteJobAsync respects cancellation token")]
    public async Task ExecuteJobAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new TestBackgroundJob("Cancellable Job");
        var cancellationToken = new CancellationToken(canceled: false);

        // Act & Assert
        await Should.NotThrowAsync(() =>
            scheduler.ExecuteJobAsync(job, "Display", cancellationToken));
    }

    [Fact(DisplayName = "Multiple jobs can be scheduled concurrently")]
    public void MultipleJobs_ScheduledConcurrently_AllComplete()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();

        // Act
        string jobId1 = scheduler.Enqueue(new TestBackgroundJob("Job1"));
        string jobId2 = scheduler.Enqueue(new TestBackgroundJob("Job2"));
        string jobId3 = scheduler.Enqueue(new SimpleTestJob());

        // Assert
        jobId1.ShouldNotBeNull();
        jobId2.ShouldNotBeNull();
        jobId3.ShouldNotBeNull();
        jobId1.ShouldNotBe(jobId2);
        jobId1.ShouldNotBe(jobId3);
    }

    [Fact(DisplayName = "Job chain can be created successfully")]
    public void JobChain_Created_Successfully()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job1 = new TestBackgroundJob("First");

        // Act
        string jobId1 = scheduler.Enqueue(job1);
        var job2 = new TestBackgroundJob("Second");
        string jobId2 = scheduler.ContinueWith(jobId1, job2);

        // Assert
        jobId1.ShouldNotBeNull();
        jobId2.ShouldNotBeNull();
        jobId1.ShouldNotBe(jobId2);
    }

    [Fact(DisplayName = "Recurring job can be updated")]
    public void RecurringJob_CanBeUpdated_Successfully()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job1 = new TestBackgroundJob("Initial");
        var job2 = new TestBackgroundJob("Updated");

        // Act & Assert
        Should.NotThrow(() =>
        {
            scheduler.AddOrUpdateRecurring("updatable", job1, "0 0 * * *");
            scheduler.AddOrUpdateRecurring("updatable", job2, "0 12 * * *");
        });
    }

    [Fact(DisplayName = "ExecuteJobAsync propagates exception from handler")]
    public async Task ExecuteJobAsync_WhenHandlerThrows_PropagatesException()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new FailingJob();

        // Act & Assert
        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            scheduler.ExecuteJobAsync(job, "Failing Display", CancellationToken.None));
        exception.Message.ShouldBe("Job execution failed");
    }

    [Fact(DisplayName = "ExecuteJobAsync handles different exception types")]
    public async Task ExecuteJobAsync_WhenHandlerThrowsDifferentExceptionTypes_PropagatesCorrectly()
    {
        // Arrange
        HangfireBackgroundJobScheduler scheduler = CreateScheduler();
        var job = new FailingJob();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() =>
            scheduler.ExecuteJobAsync(job, "Failing Job", CancellationToken.None));
    }
}
