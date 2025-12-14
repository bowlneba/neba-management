namespace Neba.Contracts.Website.Awards;

/// <summary>
/// Represents a Bowler of the Year season award for a specific category.
/// </summary>
/// <remarks>
/// Bowler of the Year recognizes overall performance across all stat-eligible tournaments during a NEBA season.
/// Points are awarded based on finishing position in tournaments, with eligibility determined by age as of each tournament date.
/// Categories include: Open, Woman, Senior (50+), SuperSenior (60+), Rookie (first-year competitor), and Youth (under 18).
/// </remarks>
/// <example>
/// {
///   "bowlerName": "John Doe",
///   "season": "2025",
///   "category": "Open Bowler of the Year"
/// }
/// </example>
public sealed record BowlerOfTheYearResponse
{
    /// <summary>
    /// Gets the full display name of the bowler who received the award.
    /// </summary>
    /// <example>"John Doe"</example>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the season for which the award was given (typically "YYYY" format, e.g., "2025").
    /// </summary>
    /// <remarks>
    /// Standard seasons run January 1 - December 31 (calendar year).
    /// Exception: "2020/2021" was a combined season due to COVID-19 tournament cancellations.
    /// </remarks>
    /// <example>"2025"</example>
    public required string Season { get; init; }

    /// <summary>
    /// Gets the Bowler of the Year category.
    /// </summary>
    /// <remarks>
    /// Categories: Open Bowler of the Year, Woman Bowler of the Year, Senior Bowler of the Year (50+),
    /// Super Senior Bowler of the Year (60+), Rookie Bowler of the Year (first-year), Youth Bowler of the Year (under 18).
    /// Age is determined as of each tournament date during the season.
    /// </remarks>
    /// <example>"Open Bowler of the Year"</example>
    public required string Category { get; init; }
}
