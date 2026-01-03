using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Awards;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Domain.Bowlers;

/// <summary>
/// A bowler represents a NEBA member who participates in tournaments. The Bowler entity serves as the
/// aggregate root for tracking competitive achievements, including titles won and season awards earned.
/// </summary>
/// <remarks>
/// The current implementation is minimal, focused on website display needs. Additional properties for member
/// management (date of birth, gender, membership information, contact details) will be added when migrating
/// from the organization management software.
/// </remarks>
public sealed class Bowler
    : Aggregate<BowlerId>
{
    /// <summary>
    /// Gets the bowler's full name (value object containing first name, last name, middle name, suffix, and optional nickname).
    /// </summary>
    public required Name Name { get; init; }

    /// <summary>
    /// Gets the legacy identifier from the existing NEBA website database (used for data migration; maintained for historical reference).
    /// </summary>
    public int? WebsiteId { get; init; }

    /// <summary>
    /// Gets the legacy identifier from the existing organization management software (used for data migration; maintained for historical reference).
    /// </summary>
    public int? ApplicationId { get; init; }

    internal Bowler()
        : base(BowlerId.New())
    {
        Titles = [];
        SeasonAwards = [];
        HallOfFameInductions = [];
    }

    // Navigation property for querying a bowler's titles. Titles are not owned by the
    // Bowler aggregate - they are owned by tournaments in the domain sense. This collection
    // exists solely for projection/querying purposes (e.g., "show all championships this
    // bowler has won"). Titles are created separately and reference both tournaments and
    // bowlers.
    //
    // Technical note: Uses a mutable backing field because when titles are created that
    // reference existing bowlers, EF Core modifies this collection during relationship
    // fixup. Using a readonly array ([]) would cause a "Collection was of a fixed size"
    // exception. The public property remains IReadOnlyCollection<Title> to maintain the
    // immutable API contract.
    private List<Title> _titles = [];

    /// <summary>
    /// Gets the collection of championship titles won by the bowler.
    /// This is a navigation property for projection only - titles are owned by tournaments.
    /// </summary>
    internal IReadOnlyCollection<Title> Titles
    {
        get => _titles;
        init => _titles = value?.ToList() ?? [];
    }

    /// <summary>
    /// Gets the collection of season awards earned by the bowler (BOTY, High Average, High Block).
    /// This is an owned collection that is part of the Bowler aggregate's state and should be
    /// mutated only through domain methods that enforce business rules.
    /// </summary>
    internal IReadOnlyCollection<SeasonAward> SeasonAwards { get; init; }

    /// <summary>
    /// Gets the collection of hall of fame inductions for this bowler.
    /// This is an owned collection that is part of the Bowler aggregate's state and should be
    /// mutated only through domain methods that enforce business rules.
    /// </summary>
    internal IReadOnlyCollection<HallOfFameInduction> HallOfFameInductions { get; init; }
}
