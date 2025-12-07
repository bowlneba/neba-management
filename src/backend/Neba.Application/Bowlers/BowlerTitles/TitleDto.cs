using Neba.Domain.Tournaments;

namespace Neba.Application.Bowlers.BowlerTitles;

/// <summary>
/// Data transfer object representing the details of a single title won by a bowler.
/// </summary>
public sealed record TitleDto
{
    /// <summary>
    /// Gets the month in which the title was won.
    /// </summary>
    public required Month Month { get; init; }

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Gets the type of tournament in which the title was won.
    /// </summary>
    public required TournamentType TournamentType { get; init; }
}
