using Neba.Domain.Abstractions;
using Neba.Domain.Bowlers;

namespace Neba.Domain.Tournaments;

/// <summary>
/// Represents a championship title won by a bowler in a specific tournament event.
/// </summary>
public sealed class Title : Entity<TitleId>
{
    /// <summary>
    /// Gets the unique identifier of the bowler who won the title.
    /// </summary>
    public BowlerId BowlerId { get; init; }

    /// <summary>
    /// Gets the bowler entity who won the title. Used internally for navigation.
    /// </summary>
    internal Bowler Bowler { get; init; } = null!;

    /// <summary>
    /// Gets the type of tournament in which the title was won.
    /// </summary>
    public TournamentType TournamentType { get; init; } = null!;

    /// <summary>
    /// Gets the month in which the title was won.
    /// </summary>
    public Month Month { get; init; } = null!;

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Title"/> class with a new unique identifier.
    /// </summary>
    internal Title()
        : base(TitleId.New())
    {}
}
