using Neba.Application;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.ListTournaments;

internal sealed class ListFutureTournamentsQueryHandler(
    IWebsiteTournamentQueryRepository tournamentQueryRepository,
    IDateTimeProvider dateTimeProvider)
        : IQueryHandler<ListFutureTournamentsQuery, IReadOnlyCollection<TournamentSummaryDto>>
{
    private readonly IWebsiteTournamentQueryRepository _tournamentQueryRepository = tournamentQueryRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<IReadOnlyCollection<TournamentSummaryDto>> HandleAsync(ListFutureTournamentsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TournamentSummaryDto> futureTournaments = await _tournamentQueryRepository.ListTournamentsAfterDateAsync(_dateTimeProvider.Today, cancellationToken);

        return futureTournaments;
    }
}
