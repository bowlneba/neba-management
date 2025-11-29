
namespace Neba.Contracts.Website.Bowlers;

/// <summary>
/// Response model representing a bowler and their total number of titles for the champions endpoint.
/// </summary>
public sealed record GetBowlerTitleCountsResponse
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the total number of titles won by the bowler.
    /// </summary>
    public int TitleCount { get; init; }
}
