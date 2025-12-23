using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;

namespace Neba.Website.Application.Documents.Bylaws;

internal sealed class BylawsSyncBackgroundJob(IBackgroundJobScheduler scheduler)
{
    public const string RecurringJobId = "sync-bylaws-to-storage";

    private readonly IBackgroundJobScheduler _scheduler = scheduler;

    public void RegisterBylawsSyncJob()
    {
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = BylawsConstants.BylawsDocumentName,
            ContainerName = BylawsConstants.BylawsContainerName,
            DocumentName = BylawsConstants.BylawsFileName,
            HubGroupName = $"{BylawsConstants.BylawsDocumentName}-refresh",
            CacheKey = $"{BylawsConstants.BylawsDocumentName}:refresh:current",
            DocumentCacheKey = BylawsConstants.BylawsDocumentName,
            TriggeredBy = "scheduled"
        };

        // Schedule to run monthly at 7:00 AM UTC on the 1st day of each month
        const string cronExpression = "0 7 1 * *";

        _scheduler.AddOrUpdateRecurring(RecurringJobId, job, cronExpression);
    }

    public string TriggerImmediateSync()
    {
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = BylawsConstants.BylawsDocumentName,
            ContainerName = BylawsConstants.BylawsContainerName,
            DocumentName = BylawsConstants.BylawsFileName,
            HubGroupName = $"{BylawsConstants.BylawsDocumentName}-refresh",
            CacheKey = $"{BylawsConstants.BylawsDocumentName}:refresh:current",
            DocumentCacheKey = BylawsConstants.BylawsDocumentName,
            TriggeredBy = "user"
        };

        return _scheduler.Enqueue(job);
    }
}
