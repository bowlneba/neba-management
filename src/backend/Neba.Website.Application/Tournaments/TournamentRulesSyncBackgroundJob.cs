using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;

namespace Neba.Website.Application.Tournaments;

internal sealed class TournamentRulesSyncBackgroundJob(IBackgroundJobScheduler scheduler)
{
    public const string RecurringJobId = "sync-tournament-rules-to-storage";

    private readonly IBackgroundJobScheduler _scheduler = scheduler;

    public void RegisterTournamentRulesSyncJob()
    {
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = TournamentRulesConstants.TournamentRulesDocumentName,
            ContainerName = TournamentRulesConstants.TournamentRulesContainerName,
            DocumentName = TournamentRulesConstants.TournamentRulesFileName,
            HubGroupName = $"{TournamentRulesConstants.TournamentRulesDocumentName}-refresh",
            CacheKey = $"{TournamentRulesConstants.TournamentRulesDocumentName}:refresh:current",
            DocumentCacheKey = TournamentRulesConstants.TournamentRulesDocumentName,
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
            DocumentKey = TournamentRulesConstants.TournamentRulesDocumentName,
            ContainerName = TournamentRulesConstants.TournamentRulesContainerName,
            DocumentName = TournamentRulesConstants.TournamentRulesFileName,
            HubGroupName = $"{TournamentRulesConstants.TournamentRulesDocumentName}-refresh",
            CacheKey = $"{TournamentRulesConstants.TournamentRulesDocumentName}:refresh:current",
            DocumentCacheKey = TournamentRulesConstants.TournamentRulesDocumentName,
            TriggeredBy = "user"
        };

        return _scheduler.Enqueue(job);
    }
}
