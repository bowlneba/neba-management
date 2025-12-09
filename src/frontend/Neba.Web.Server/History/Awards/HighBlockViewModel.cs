namespace Neba.Web.Server.History.Awards;

/// <summary>
/// View model representing a high block award for a bowler in a given season.
/// </summary>
public sealed record HighBlockViewModel
{
    /// <summary>
    /// The season (e.g., "2025") in which the high block was achieved.
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// The name of the bowler who achieved the high block score.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// The high block score achieved by the bowler.
    /// </summary>
    public required int Score { get; init; }
}
