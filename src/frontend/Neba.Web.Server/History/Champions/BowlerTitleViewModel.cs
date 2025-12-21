using Neba.Domain.Identifiers;

namespace Neba.Web.Server.History.Champions;

/// <summary>
/// Represents a single title won by a bowler.
/// </summary>
public sealed record BowlerTitleViewModel
{
    /// <summary>
    /// Gets the unique identifier for the bowler.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the month in which the title was won (1-12).
    /// </summary>
    public required int TournamentMonth { get; init; }

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    public required int TournamentYear { get; init; }

    /// <summary>
    /// Gets the type of tournament in which the title was won.
    /// </summary>
    public required string TournamentType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the bowler is inducted into the Hall of Fame.
    /// </summary>
    public bool HallOfFame { get; init; }
}
