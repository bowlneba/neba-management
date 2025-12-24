namespace Neba.Website.Application.Tournaments.TournamentRules;

/// <summary>
/// Defines operations for scheduling and triggering tournament rules document sync jobs.
/// </summary>
internal interface ITournamentRulesSyncBackgroundJob
{
    /// <summary>
    /// Triggers an immediate sync of the tournament rules document to storage.
    /// </summary>
    /// <returns>The job ID of the enqueued sync job.</returns>
    string TriggerImmediateSync();
}
