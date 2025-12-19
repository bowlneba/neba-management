using ErrorOr;
using Neba.Application.Messaging;
using Neba.Domain.Identifiers;

namespace Neba.Application.Bowlers.BowlerTitles;

/// <summary>
/// Query to retrieve the detailed titles for a specific bowler.
/// </summary>
public sealed record BowlerTitlesQuery
    : IQuery<ErrorOr<BowlerTitlesDto>>
{
    /// <summary>
    /// Gets the unique identifier of the bowler whose titles are being requested.
    /// </summary>
    public required BowlerId BowlerId { get; init; }
}
