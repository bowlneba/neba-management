namespace Neba.Contracts.Website.Awards;

/// <summary>
/// Represents a Bowler of the Year award recognition for a specific season and category.
/// </summary>
/// <remarks>
/// This response contains information about bowlers who have been recognized as the best performer
/// in their respective categories for a given season. Categories typically include Open, Woman, and Senior divisions.
/// </remarks>
/// <example>
/// {
///   "bowlerName": "John Doe",
///   "season": "2024-2025",
///   "category": "Open"
/// }
/// </example>
public sealed record BowlerOfTheYearResponse
{
    /// <summary>
    /// Gets the full display name of the bowler who received the award.
    /// </summary>
    /// <example>John Doe</example>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the bowling season in which the award was given, formatted as a year range.
    /// </summary>
    /// <remarks>
    /// The season format represents the bowling year which typically spans across two calendar years.
    /// </remarks>
    /// <example>2024-2025</example>
    public required string Season { get; init; }

    /// <summary>
    /// Gets the award category or division.
    /// </summary>
    /// <remarks>
    /// Common categories include Open (unrestricted division), Woman (women's division), and Senior (senior division).
    /// </remarks>
    /// <example>Open</example>
    public required string Category { get; init; }
}
