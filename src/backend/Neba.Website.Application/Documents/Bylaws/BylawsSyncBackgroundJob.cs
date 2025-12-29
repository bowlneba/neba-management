using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;

namespace Neba.Website.Application.Documents.Bylaws;

internal sealed class BylawsSyncBackgroundJob(IBackgroundJobScheduler scheduler) : IBylawsSyncBackgroundJob
{
    public const string RecurringJobId = "sync-bylaws-to-storage";

    public void RegisterBylawsSyncJob()
    {
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = BylawsConstants.DocumentKey,
            Container = BylawsConstants.Container,
            Path = BylawsConstants.Path,
            HubGroupName = $"{BylawsConstants.DocumentKey}-refresh",
            CacheKey = BylawsConstants.JobStateCacheKey,
            DocumentCacheKey = BylawsConstants.ContentCacheKey,
            TriggeredBy = "scheduled"
        };

        // Schedule to run monthly at 7:00 AM UTC on the 1st day of each month
        const string cronExpression = "0 7 1 * *";

        scheduler.AddOrUpdateRecurring(RecurringJobId, job, cronExpression);
    }

    public string TriggerImmediateSync()
    {
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = BylawsConstants.DocumentKey,
            Container = BylawsConstants.Container,
            Path = BylawsConstants.Path,
            HubGroupName = $"{BylawsConstants.DocumentKey}-refresh",
            CacheKey = BylawsConstants.JobStateCacheKey,
            DocumentCacheKey = BylawsConstants.ContentCacheKey,
            TriggeredBy = "user"
        };

        return scheduler.Enqueue(job);
    }
}
