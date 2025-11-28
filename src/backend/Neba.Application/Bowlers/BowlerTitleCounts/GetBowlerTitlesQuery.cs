using Neba.Application.Abstractions.Messaging;
using Neba.Domain.Bowlers;

namespace Neba.Application.Bowlers.BowlerTitleCounts;

/// <summary>
/// Query to retrieve the detailed titles for a specific bowler.
/// </summary>
public sealed record GetBowlerTitlesQuery
    : IQuery<BowlerTitlesDto?>
{
    /// <summary>
    /// Gets the unique identifier of the bowler whose titles are being requested.
    /// </summary>
    public required BowlerId BowlerId { get; init; }
}
