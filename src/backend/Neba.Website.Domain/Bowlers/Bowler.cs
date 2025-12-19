using Neba.Domain;
using Neba.Domain.Awards;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;

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
    }

    /// <summary>
    /// Gets the read-only collection of championship titles won by the bowler.
    /// </summary>
    internal IReadOnlyCollection<Title> Titles { get; init; }

    /// <summary>
    /// Gets the read-only collection of season awards earned by the bowler (BOTY, High Average, High Block).
    /// </summary>
    internal IReadOnlyCollection<SeasonAward> SeasonAwards { get; init; }
}
