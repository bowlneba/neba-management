namespace Neba.Contracts.Website.Bowlers;

/// <summary>
/// Represents a summary response containing a bowler's unique identifier, name, and total title count.
/// </summary>
public sealed record BowlerTitleSummaryResponse
{
    /// <summary>
    /// The unique identifier of the bowler.
    /// </summary>
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// The full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// The total number of titles won by the bowler.
    /// </summary>
    public required int TitleCount { get; init; }
}
