namespace Neba.Contracts.Website.Awards;


/// <summary>
/// Represents a response containing Bowler of the Year award information.
/// </summary>
public sealed record BowlerOfTheYearResponse
{
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
    public required string Category { get; init; }
}
