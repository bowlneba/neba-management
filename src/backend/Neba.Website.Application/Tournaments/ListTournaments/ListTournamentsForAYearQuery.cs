using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.ListTournaments;

/// <summary>
/// Query to retrieve all tournaments for a specific year.
/// </summary>
public sealed record ListTournamentsForAYearQuery
    : ICachedQuery<IReadOnlyCollection<TournamentSummaryDto>>
{
    /// <summary>
    /// Gets the year for which to retrieve tournaments.
    /// </summary>
    public required int Year { get; init; }

    /// <inheritdoc />
    public string Key
        => CacheKeys.Queries.Build(nameof(ListTournamentsForAYearQuery), Year);

    /// <inheritdoc />
    public TimeSpan Expiry
        => TimeSpan.FromDays(14);

    /// <inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.AllTournaments();
}
