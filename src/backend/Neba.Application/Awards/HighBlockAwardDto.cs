using Neba.Domain.Awards;

namespace Neba.Application.Awards;

/// <summary>
/// Data Transfer Object representing the recipient of a High Block award for a season.
/// </summary>
public sealed record HighBlockAwardDto
{
    /// <summary>
    /// Gets the unique identifier for this season award.
    /// </summary>
    public required SeasonAwardId Id { get; init; }

    /// <summary>
    /// Gets the season in which the award was earned.
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// Gets the name of the bowler who received the award.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the score that qualified for the High Block award.
    /// </summary>
    public required int Score { get; init; }
}
