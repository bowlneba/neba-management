namespace Neba.Contracts.Website.Titles;

/// <summary>
/// Represents a summary of a bowler's title achievements, including their identity and total title count.
/// </summary>
/// <remarks>
/// This response is typically used in list views or leaderboards where only aggregate title information is needed,
/// without the full details of each individual title.
/// </remarks>
/// <example>
/// {
///   "bowlerId": "123e4567-e89b-12d3-a456-426614174000",
///   "bowlerName": "John Doe",
///   "titleCount": 5
/// }
/// </example>
public sealed record TitleSummaryResponse
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    /// <example>"123e4567-e89b-12d3-a456-426614174000"</example>
    public required Ulid BowlerId { get; init; }

    /// <summary>
    /// Gets the full display name of the bowler.
    /// </summary>
    /// <example>"John Doe"</example>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the total number of titles won by the bowler across all tournaments and years.
    /// </summary>
    /// <example>5</example>
    public required int TitleCount { get; init; }
}
