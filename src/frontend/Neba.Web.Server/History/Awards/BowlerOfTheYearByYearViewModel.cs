namespace Neba.Web.Server.History.Awards;

/// <summary>
/// Represents Bowler of the Year winners for a specific season, grouped by category.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Season"/> is usually a single year (e.g. "2025"), but may be a range (e.g. "2020-2021") for split seasons.
/// </para>
/// <para>
/// <see cref="WinnersByCategory"/> maps award categories (such as "Overall", "Senior", "Junior") to the name of the winning bowler for each category.
/// </para>
/// </remarks>
public sealed record BowlerOfTheYearByYearViewModel
{
    /// <summary>
    /// The season for which the Bowler of the Year awards apply.
    /// Usually a single year (e.g. "2025"); may be a range for split seasons (e.g. "2020-2021").
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// Maps each award category to the name of the winning bowler for that category.
    /// For example: { "Open": "Jane Smith", "Senior": "John Doe" }
    /// </summary>
    public required IEnumerable<KeyValuePair<string, string>> WinnersByCategory { get; init; }
}
