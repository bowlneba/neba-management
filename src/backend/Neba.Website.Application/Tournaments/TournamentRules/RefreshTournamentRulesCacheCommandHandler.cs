using ErrorOr;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.TournamentRules;

internal sealed class RefreshTournamentRulesCacheCommandHandler(TournamentRulesSyncBackgroundJob tournamentRulesSyncBackgroundJob)
        : ICommandHandler<RefreshTournamentRulesCacheCommand, string>
{
    private readonly TournamentRulesSyncBackgroundJob _tournamentRulesSyncBackgroundJob = tournamentRulesSyncBackgroundJob;

    public Task<ErrorOr<string>> HandleAsync(
        RefreshTournamentRulesCacheCommand command,
        CancellationToken cancellationToken)
    {
        string jobId = _tournamentRulesSyncBackgroundJob.TriggerImmediateSync();

        return Task.FromResult<ErrorOr<string>>(jobId);
    }
}
