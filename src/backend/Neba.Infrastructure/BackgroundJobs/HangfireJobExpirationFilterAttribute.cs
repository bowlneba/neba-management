using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Neba.Infrastructure.BackgroundJobs;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class JobExpirationFilterAttribute(HangfireSettings settings)
        : JobFilterAttribute, IApplyStateFilter
{
    public HangfireSettings Settings { get; } = settings;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is SucceededState)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(Settings.SucceededJobsRetentionDays);
        }
        else if (context.NewState is FailedState)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(Settings.FailedJobsRetentionDays);
        }
        else if (context.NewState is DeletedState)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(Settings.DeletedJobsRetentionDays);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No implementation needed
    }
}

