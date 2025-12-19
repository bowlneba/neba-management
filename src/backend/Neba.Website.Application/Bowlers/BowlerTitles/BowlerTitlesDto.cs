using Neba.Domain.Identifiers;
using Neba.Website.Application.Tournaments;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

/// <summary>
/// Data transfer object representing a bowler and the details of each title they have won.
/// </summary>
public sealed record BowlerTitlesDto
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the collection of titles won by the bowler, including month, year, and tournament type for each title.
    /// </summary>
    public required IReadOnlyCollection<TitleDto> Titles { get; init; }
}


