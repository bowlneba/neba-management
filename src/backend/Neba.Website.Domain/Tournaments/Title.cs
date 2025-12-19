using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Bowlers;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// A title represents a championship win in a NEBA tournament. Titles are awarded to the winner(s)
/// of each tournament event and serve as a permanent record of competitive achievement.
/// Earning at least one title grants eligibility for the Tournament of Champions.
/// </summary>
/// <remarks>
/// For team tournaments (Doubles, Trios), each team member receives their own individual Title record.
/// Titles are permanent records and are never deleted, even for inactive tournament formats.
/// </remarks>
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
    /// Gets the specific tournament format that was won (Singles, Doubles, Trios, etc.).
    /// </summary>
    public TournamentType TournamentType { get; init; } = null!;

    /// <summary>
    /// Gets the month in which the title was won (provides multiple format options: numeric, full name, abbreviated).
    /// </summary>
    public Month Month { get; init; } = null!;

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    public int Year { get; init; }

    internal Title()
        : base(TitleId.New())
    { }
}
