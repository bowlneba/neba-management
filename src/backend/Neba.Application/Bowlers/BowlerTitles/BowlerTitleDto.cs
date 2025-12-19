using Neba.Domain.Bowlers;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;

namespace Neba.Application.Bowlers.BowlerTitles;

/// <summary>
/// Data transfer object representing a single title won by a bowler, including bowler and tournament details.
/// </summary>
public sealed record BowlerTitleDto
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the month in which the title was won.
    /// </summary>
    public required Month TournamentMonth { get; init; }

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    public required int TournamentYear { get; init; }

    /// <summary>
    /// Gets the type of tournament in which the title was won.
    /// </summary>
    public required TournamentType TournamentType { get; init; }
}
