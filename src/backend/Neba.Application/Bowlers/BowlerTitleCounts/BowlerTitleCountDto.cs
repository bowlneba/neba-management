using Neba.Domain.Bowlers;

namespace Neba.Application.Bowlers.BowlerTitleCounts;

/// <summary>
/// Data transfer object representing a bowler and their total number of titles.
/// </summary>
public sealed record BowlerTitleCountDto
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required BowlerId Id { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the total number of titles won by the bowler.
    /// </summary>
    public required int TitleCount { get; init; }
}
