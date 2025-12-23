using System.ComponentModel.DataAnnotations;

namespace Neba.Infrastructure.BackgroundJobs;

internal sealed record HangfireSettings
{
    [Range(1, 100, ErrorMessage = "WorkerCount must be between 1 and 100.")]
    public required int WorkerCount { get; init; }

    [Range(1, 365, ErrorMessage = "SucceededJobsRetentionDays must be between 1 and 365.")]
    public required int SucceededJobsRetentionDays { get; init; }

    [Range(1, 365, ErrorMessage = "DeletedJobsRetentionDays must be between 1 and 365.")]
    public required int DeletedJobsRetentionDays { get; init; }

    [Range(1, 365, ErrorMessage = "FailedJobsRetentionDays must be between 1 and 365.")]
    public required int FailedJobsRetentionDays { get; init; }
}
