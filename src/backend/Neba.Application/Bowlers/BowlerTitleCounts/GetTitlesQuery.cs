using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitleCounts;

/// <summary>
/// Query for retrieving a collection of all bowler titles, including bowler and tournament details for each title.
/// </summary>
public sealed record GetTitlesQuery
    : IQuery<IReadOnlyCollection<BowlerTitleDto>>;
