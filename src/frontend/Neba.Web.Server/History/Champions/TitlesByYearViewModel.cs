namespace Neba.Web.Server.History.Champions;

/// <summary>
/// Represents a collection of titles grouped by year.
/// </summary>
public sealed record TitlesByYearViewModel
{
    /// <summary>
    /// Gets the year for this group of titles.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Gets the collection of titles won in this year.
    /// </summary>
    public required IReadOnlyCollection<BowlerTitleViewModel> Titles { get; init; }
}
