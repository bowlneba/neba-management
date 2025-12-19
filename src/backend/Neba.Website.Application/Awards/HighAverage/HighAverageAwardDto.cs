using Neba.Domain.Identifiers;

namespace Neba.Website.Application.Awards.HighAverage;

/// <summary>
/// Data transfer object representing a high-average award for a bowler in a specific season.
/// </summary>
public sealed record HighAverageAwardDto
{
    /// <summary>
    /// The unique identifier for the season award.
    /// </summary>
    public required SeasonAwardId Id { get; init; }

    /// <summary>
    /// The season when the award was earned (for example, "2024/2025").
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// The full name of the bowler who received the award.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// The bowler's average for the season.
    /// </summary>
    public required decimal Average { get; init; }

    /// <summary>
    /// Optional: number of games played that contributed to the average.
    /// </summary>
    public int? Games { get; init; }

    /// <summary>
    /// Optional: number of tournaments in which the bowler participated during the season.
    /// </summary>
    public int? Tournaments { get; init; }
}
