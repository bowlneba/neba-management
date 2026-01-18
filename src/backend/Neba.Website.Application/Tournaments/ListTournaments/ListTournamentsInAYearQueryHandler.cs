using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.ListTournaments;

internal sealed class ListTournamentsInAYearQueryHandler(
    IWebsiteTournamentQueryRepository tournamentQueryRepository)
        : IQueryHandler<ListTournamentInAYearQuery, IReadOnlyCollection<TournamentSummaryDto>>
{
    private readonly IWebsiteTournamentQueryRepository _tournamentQueryRepository = tournamentQueryRepository;

    public async Task<IReadOnlyCollection<TournamentSummaryDto>> HandleAsync(ListTournamentInAYearQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TournamentSummaryDto> futureTournaments = await _tournamentQueryRepository.ListTournamentsInYearAsync(query.Year, cancellationToken);

        return futureTournaments;
    }
}
