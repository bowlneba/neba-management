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
public sealed class Title
    : Entity<TitleId>
{
    /// <summary>
    /// Gets the identifier of the bowler who won the title.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    internal Bowler Bowler { get; init; } = null!;

    internal Tournament Tournament { get; init; } = null!;

    internal TournamentId TournamentId { get; init; }

    internal Title()
        : base(TitleId.New())
    { }
}
