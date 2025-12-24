using ErrorOr;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.TournamentRules;

internal sealed class RefreshTournamentRulesCacheCommandHandler(ITournamentRulesSyncBackgroundJob tournamentRulesSyncBackgroundJob)
        : ICommandHandler<RefreshTournamentRulesCacheCommand, string>
{
    private readonly ITournamentRulesSyncBackgroundJob _tournamentRulesSyncBackgroundJob = tournamentRulesSyncBackgroundJob;

    public Task<ErrorOr<string>> HandleAsync(
        RefreshTournamentRulesCacheCommand command,
        CancellationToken cancellationToken)
    {
        string jobId = _tournamentRulesSyncBackgroundJob.TriggerImmediateSync();

        return Task.FromResult<ErrorOr<string>>(jobId);
    }
}
