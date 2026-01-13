using Neba.Application;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.ListTournaments;

internal sealed class ListFutureTournamentsQueryHandler
    : IQueryHandler<ListFutureTournamentsQuery, IReadOnlyCollection<TournamentSummaryDto>>
{
    private readonly IWebsiteTournamentQueryRepository _tournamentQueryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ListFutureTournamentsQueryHandler(
        IWebsiteTournamentQueryRepository tournamentQueryRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _tournamentQueryRepository = tournamentQueryRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyCollection<TournamentSummaryDto>> HandleAsync(ListFutureTournamentsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TournamentSummaryDto> futureTournaments = await _tournamentQueryRepository.ListTournamentsAfterDateAsync(_dateTimeProvider.Today, cancellationToken);

        return futureTournaments;
    }
}
