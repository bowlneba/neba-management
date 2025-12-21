using Neba.Domain.Identifiers;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

/// <summary>
/// Represents a summary of a bowler's titles, including the bowler's ID, name, and total title count.
/// </summary>
public sealed record BowlerTitleSummaryDto
{
    /// <summary>
    /// The unique identifier of the bowler.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// The full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// The total number of titles won by the bowler.
    /// </summary>
    public required int TitleCount { get; init; }
}
