namespace Neba.Web.Server.History.Awards;


/// <summary>
/// Represents the high average award details for a bowler in a given season.
/// </summary>
public sealed record HighAverageViewModel
{
    /// <summary>
    /// The bowling season for which the high average was awarded, formatted as a year range.
    /// </summary>
    /// <example>2024/2025</example>
    public required string Season { get; init; }

    /// <summary>
    /// The full display name of the bowler who achieved the high average.
    /// </summary>
    /// <example>Jane Doe</example>
    public required string BowlerName { get; init; }

    /// <summary>
    /// The average score achieved by the bowler during the season.
    /// </summary>
    /// <example>221.4</example>
    public required decimal Average { get; init; }

    /// <summary>
    /// The total number of games bowled by the award recipient during the season, if available.
    /// </summary>
    /// <example>42</example>
    public int? GamesBowled { get; init; }

    /// <summary>
    /// The total number of tournaments bowled by the award recipient during the season, if available.
    /// </summary>
    /// <example>5</example>
    public int? TournamentsBowled { get; init; }
}
