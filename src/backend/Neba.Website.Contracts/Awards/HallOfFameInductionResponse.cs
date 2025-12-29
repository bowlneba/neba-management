namespace Neba.Website.Contracts.Awards;

/// <summary>
/// Response contract describing a Hall of Fame induction returned by the website API.
/// </summary>
public sealed record HallOfFameInductionResponse
{
    /// <summary>
    /// The year the induction took place.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// The inducted bowler's display name.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Optional public URL to the bowler's photo.
    /// </summary>
    public Uri? PhotoUrl { get; init; }

    /// <summary>
    /// The list of category names associated with this induction.
    /// </summary>
    public required IReadOnlyCollection<string> Categories { get; init; }
}
