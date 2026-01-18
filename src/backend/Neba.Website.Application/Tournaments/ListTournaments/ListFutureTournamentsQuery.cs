using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.ListTournaments;

/// <summary>
/// Query to list future tournaments.
/// </summary>
public sealed record ListFutureTournamentsQuery
    : IQuery<IReadOnlyCollection<TournamentSummaryDto>>;
