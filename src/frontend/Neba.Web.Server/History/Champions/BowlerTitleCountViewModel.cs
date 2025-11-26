namespace Neba.Web.Server.History.Champions;

/// <summary>
/// Represents a bowler with their total title count.
/// </summary>
public sealed record BowlerTitleCountViewModel
{
    /// <summary>
    /// Gets the unique identifier for the bowler.
    /// </summary>
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the total number of tournament titles won by the bowler.
    /// </summary>
    public required int Titles { get; init; }
}
