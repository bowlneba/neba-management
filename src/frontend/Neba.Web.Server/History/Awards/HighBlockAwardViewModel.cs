namespace Neba.Web.Server.History.Awards;

/// <summary>
/// View model representing a high block award for a season.
/// Supports multiple bowlers in case of ties.
/// </summary>
public sealed record HighBlockAwardViewModel
{
    /// <summary>
    /// The season (e.g., "2025") in which the high block was achieved.
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// The name(s) of the bowler(s) who achieved the high block score.
    /// Will contain multiple entries if there was a tie.
    /// </summary>
    public required IEnumerable<string> Bowlers { get; init; }

    /// <summary>
    /// The high block score achieved by the bowler(s).
    /// </summary>
    public required int Score { get; init; }
}
