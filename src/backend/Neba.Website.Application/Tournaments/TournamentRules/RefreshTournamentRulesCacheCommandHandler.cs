using ErrorOr;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.TournamentRules;

internal sealed class RefreshTournamentRulesCacheCommandHandler(ITournamentRulesSyncBackgroundJob tournamentRulesSyncBackgroundJob)
        : ICommandHandler<RefreshTournamentRulesCacheCommand, string>
{

    public Task<ErrorOr<string>> HandleAsync(
        RefreshTournamentRulesCacheCommand command,
        CancellationToken cancellationToken)
    {
        string jobId = tournamentRulesSyncBackgroundJob.TriggerImmediateSync();

        return Task.FromResult<ErrorOr<string>>(jobId);
    }
}
