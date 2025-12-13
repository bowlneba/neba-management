namespace Neba.Contracts.Website.Awards;

/// <summary>
/// Represents a High Average award given to a bowler for achieving the highest per-game average over a season.
/// </summary>
/// <remarks>
/// The High Average award recognizes the bowler with the highest average pinfall per game across the specified season.
/// This response contains award identification, bowler display name and summary statistics used for public website listings.
/// </remarks>
/// <example>
/// {
///   "id": "d2f1e8a5-3b9a-4c6b-8f2a-1a2b3c4d5e6f",
///   "bowlerName": "Jane Doe",
///   "season": "2024/2025",
///   "average": 188.75,
///   "games": 42,
///   "tournaments": 3
/// }
/// </example>
public sealed record HighAverageAwardResponse
{
    /// <summary>
    /// Gets the unique identifier of the High Average award record.
    /// </summary>
    /// <example>"d2f1e8a5-3b9a-4c6b-8f2a-1a2b3c4d5e6f"</example>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the full display name of the bowler who received the award.
    /// </summary>
    /// <remarks>
    /// This name is suitable for public display on the website and in award listings.
    /// </remarks>
    /// <example>"Jane Doe"</example>
    public required string BowlerName { get; set; }

    /// <summary>
    /// Gets the bowling season for which the award was given, formatted as a year range.
    /// </summary>
    /// <remarks>
    /// The season format represents the bowling year which typically spans across two calendar years.
    /// </remarks>
    /// <example>"2024-2025"</example>
    public required string Season { get; set; }

    /// <summary>
    /// Gets the average pinfall per game for the award period.
    /// </summary>
    /// <remarks>
    /// The average is calculated as total pinfall divided by games bowled and is typically presented with two decimal places.
    /// Use <see cref="decimal"/> to preserve precision for averages.
    /// </remarks>
    /// <example>188.75</example>
    public required decimal Average { get; init; }

    /// <summary>
    /// Gets the number of games included in the average calculation, if available.
    /// </summary>
    /// <remarks>
    /// This value is optional and may be null when the data source does not provide a games count.
    /// </remarks>
    /// <example>42</example>
    public int? Games { get; init; }

    /// <summary>
    /// Gets the number of tournaments included in the season summary, if available.
    /// </summary>
    /// <remarks>
    /// This field is optional and helps consumers show context for the average (e.g., how many events contributed).
    /// </remarks>
    /// <example>3</example>
    public int? Tournaments { get; init; }
}
