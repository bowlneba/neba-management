namespace Neba.Contracts.Website.Awards;

/// <summary>
/// Represents a High Average season award given to the bowler with the highest average across stat-eligible tournaments.
/// </summary>
/// <remarks>
/// The High Average award is given for achieving the highest average score during the season across stat-eligible tournaments.
/// Eligibility requires a minimum of 4.5 × (number of stat-eligible tournaments completed) games, with decimals dropped.
/// All games bowled in stat-eligible tournaments count toward the average (exception: baker team finals do not count).
/// This response includes the award winner's statistics for public website display.
/// </remarks>
/// <example>
/// {
///   "id": "01JEFQH3X8KZ9M2N4P5Q7R8T9V",
///   "bowlerName": "Jane Doe",
///   "season": "2025",
///   "average": 188.75,
///   "games": 42,
///   "tournaments": 9
/// }
/// </example>
public sealed record HighAverageAwardResponse
{
    /// <summary>
    /// Gets the unique identifier of the High Average award record.
    /// </summary>
    /// <example>"01JEFQH3X8KZ9M2N4P5Q7R8T9V"</example>
    public required Ulid Id { get; init; }

    /// <summary>
    /// Gets the full display name of the bowler who received the award.
    /// </summary>
    /// <remarks>
    /// This name is suitable for public display on the website and in award listings.
    /// </remarks>
    /// <example>"Jane Doe"</example>
    public required string BowlerName { get; set; }

    /// <summary>
    /// Gets the season for which the award was given (typically "YYYY" format, e.g., "2025").
    /// </summary>
    /// <remarks>
    /// Standard seasons run January 1 - December 31 (calendar year).
    /// Exception: "2020/2021" was a combined season due to COVID-19 tournament cancellations.
    /// </remarks>
    /// <example>"2025"</example>
    public required string Season { get; set; }

    /// <summary>
    /// Gets the bowler's average pinfall per game across all stat-eligible tournaments during the season.
    /// </summary>
    /// <remarks>
    /// The average is calculated as total pinfall divided by total games bowled in stat-eligible tournaments.
    /// Uses decimal type to preserve precision for accurate average display.
    /// </remarks>
    /// <example>188.75</example>
    public required decimal Average { get; init; }

    /// <summary>
    /// Gets the total number of games bowled in stat-eligible tournaments during the season.
    /// </summary>
    /// <remarks>
    /// Provides context for comparison between high average winners across different seasons.
    /// Optional field - may be null for historical data where game counts were not tracked.
    /// </remarks>
    /// <example>42</example>
    public int? Games { get; init; }

    /// <summary>
    /// Gets the number of stat-eligible tournaments participated in during the season.
    /// </summary>
    /// <remarks>
    /// Provides context for the average and helps demonstrate consistent performance across multiple events.
    /// Optional field - may be null for historical data where tournament counts were not tracked.
    /// </remarks>
    /// <example>9</example>
    public int? Tournaments { get; init; }
}
