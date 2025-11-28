using Neba.Contracts.History.Titles;

namespace Neba.Contracts.History.Champions;

/// <summary>
/// Response containing a bowler's identity, name, and the collection of titles they have won.
/// </summary>
public sealed record GetBowlerTitlesResponse
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the collection of titles won by the bowler.
    /// </summary>
    public required IReadOnlyCollection<TitlesResponse> Titles { get; init; }
}
