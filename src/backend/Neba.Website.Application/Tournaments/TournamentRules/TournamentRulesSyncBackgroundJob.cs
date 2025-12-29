using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;

namespace Neba.Website.Application.Tournaments.TournamentRules;

internal sealed class TournamentRulesSyncBackgroundJob(IBackgroundJobScheduler scheduler) : ITournamentRulesSyncBackgroundJob
{
    public const string RecurringJobId = "sync-tournament-rules-to-storage";

    public void RegisterTournamentRulesSyncJob()
    {
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = TournamentRulesConstants.DocumentKey,
            Container = TournamentRulesConstants.Container,
            Path = TournamentRulesConstants.Path,
            HubGroupName = $"{TournamentRulesConstants.DocumentKey}-refresh",
            CacheKey = TournamentRulesConstants.JobStateCacheKey,
            DocumentCacheKey = TournamentRulesConstants.ContentCacheKey,
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
            DocumentKey = TournamentRulesConstants.DocumentKey,
            Container = TournamentRulesConstants.Container,
            Path = TournamentRulesConstants.Path,
            HubGroupName = $"{TournamentRulesConstants.DocumentKey}-refresh",
            CacheKey = TournamentRulesConstants.JobStateCacheKey,
            DocumentCacheKey = TournamentRulesConstants.ContentCacheKey,
            TriggeredBy = "user"
        };

        return scheduler.Enqueue(job);
    }
}
