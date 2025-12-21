namespace Neba.Infrastructure.BackgroundJobs;

internal sealed record HangfireSettings
{
    public required int WorkerCount { get; init; }

    public required int SucceededJobsRetentionDays { get; init; }

    public required int DeletedJobsRetentionDays { get; init; }

    public required int FailedJobsRetentionDays { get; init; }
}

