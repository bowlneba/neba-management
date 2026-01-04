using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Domain.BowlingCenters;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Represents a bowling tournament aggregate root.
/// </summary>
public sealed class Tournament
    : Aggregate<TournamentId>
{
    /// <summary>
    /// Gets the name of the tournament.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the start date of the tournament.
    /// </summary>
    public DateOnly StartDate { get; init; }

    /// <summary>
    /// Gets the end date of the tournament.
    /// </summary>
    public DateOnly EndDate { get; init; }

    /// <summary>
    /// Gets the identifier of the bowling center where the tournament is held.
    /// </summary>
    public BowlingCenterId? BowlingCenterId { get; init; }

    /// <summary>
    /// Gets the bowling center navigation property.
    /// </summary>
    internal BowlingCenter? BowlingCenter { get; init; }

    /// <summary>
    /// Gets the type of the tournament.
    /// </summary>
    public required TournamentType TournamentType { get; init; }

    /// <summary>
    /// Gets the lane pattern used in the tournament.
    /// </summary>
    public LanePattern? LanePattern { get; init; }

    /// <summary>
    /// Gets the website identifier associated with the tournament.
    /// </summary>
    public int? WebsiteId { get; init; }

    /// <summary>
    /// Gets the application identifier associated with the tournament.
    /// </summary>
    public int? ApplicationId { get; init; }

    // Owned collection of champions. The Tournament aggregate owns the business rules
    // for champions (e.g., a singles tournament can only have one champion, doubles must
    // have exactly two, etc.). These rules are part of the tournament's invariants.
    //
    // Technical note: Uses a mutable backing field because when titles are created that
    // reference existing tournaments, EF Core modifies this collection during relationship
    // fixup. Using a readonly array ([]) would cause a "Collection was of a fixed size"
    // exception. The public property remains IReadOnlyCollection<Title> to maintain the
    // immutable API contract.
    private readonly List<Title> _champions = [];

    /// <summary>
    /// Gets the collection of champions (titles) awarded in this tournament.
    /// The tournament owns the business rules for how many and what type of champions are valid.
    /// </summary>
    public IReadOnlyCollection<Title> Champions
    {
        get => _champions;
        init => _champions = value?.ToList() ?? [];
    }

    internal Tournament()
        : base(TournamentId.New())
    {}

}
