using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

/// <summary>
/// Query for retrieving a collection of all bowler titles, including bowler and tournament details for each title.
/// </summary>
public sealed record ListBowlerTitlesQuery
    : IQuery<IReadOnlyCollection<BowlerTitleDto>>;
