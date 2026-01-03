using Neba.Domain.Tournaments;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Represents a lane pattern used in a tournament.
/// </summary>
public sealed record LanePattern
{
    /// <summary>
    /// Gets the name of the lane pattern.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the length of the lane pattern.
    /// </summary>
    public required PatternLengthCategory LengthCategory { get; init; }

    /// <summary>
    /// Gets the ratio of the lane pattern.
    /// </summary>
    public required PatternRatioCategory RatioCategory { get; init; }
}
