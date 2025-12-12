using Neba.Domain.Awards;
using Neba.Domain.Bowlers;

namespace Neba.Application.Awards.BowlerOfTheYear;

/// <summary>
/// Data transfer object representing a Bowler of the Year award.
/// </summary>
public sealed record BowlerOfTheYearAwardDto
{
    /// <summary>
    /// The unique identifier of the Bowler of the Year award.
    /// </summary>
    public required SeasonAwardId Id { get; init; }

    /// <summary>
    /// The unique identifier of the bowler who received the award.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// The name of the bowler who received the award.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// The season in which the award was given (e.g., "2024-2025").
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// The category of the Bowler of the Year award (e.g., Open, Woman, Senior).
    /// </summary>
    public required BowlerOfTheYearCategory Category { get; init; }
}
