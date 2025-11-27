namespace Neba.Contracts.History.Champions;

/// <summary>
/// Response containing a collection of bowlers and their title counts for the champions endpoint.
/// </summary>
public sealed record GetBowlerTitleCountsResponse
{
    /// <summary>
    /// Gets the collection of bowlers and their title counts.
    /// </summary>
    public required IReadOnlyCollection<GetBowlerTitleCountsResponseModel> Bowlers { get; init; }
}
