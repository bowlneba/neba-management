using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitleCounts;

/// <summary>
/// Query to retrieve a collection of bowlers and their total title counts.
/// </summary>
public sealed record GetBowlerTitleCountsQuery
    : IQuery<IReadOnlyCollection<BowlerTitleCountDto>>;
