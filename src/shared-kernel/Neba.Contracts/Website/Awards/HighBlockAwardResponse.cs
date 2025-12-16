namespace Neba.Contracts.Website.Awards;

/// <summary>
/// Represents a High 5-Game Block season award given to the bowler with the highest 5-game qualifying block score.
/// </summary>
/// <remarks>
/// The High 5-Game Block award is given for achieving the highest total pinfall in a 5-game qualifying block
/// during a stat-eligible tournament within the season. The block must be from the qualification rounds before match play.
/// This response includes the award winner's score for public website display.
/// </remarks>
/// <example>
/// {
///   "id": "01JEFQH3X8KZ9M2N4P5Q7R8T9V",
///   "bowlerName": "Jane Doe",
///   "season": "2025",
///   "score": 1123
/// }
/// </example>
public sealed record HighBlockAwardResponse
{
    /// <summary>
    /// Gets the unique identifier of the High 5-Game Block award record.
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
    /// Gets the total pinfall score achieved in the highest 5-game qualifying block during the season.
    /// </summary>
    /// <remarks>
    /// This score represents the sum of pins knocked down across 5 consecutive games during a tournament
    /// qualification round. Only qualifying blocks from stat-eligible tournaments are considered.
    /// </remarks>
    /// <example>1123</example>
    public required int Score { get; init; }
}
