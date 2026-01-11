using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Domain.Bowlers;
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
    /// Gets the application identifier associated with the tournament.
    /// </summary>
    public int? ApplicationId { get; init; }

    /// <summary>
    /// Gets the number of entries in the tournament.
    /// </summary>
    public int? EntryCount { get; init; }

    // Collection of champion bowlers. The Tournament aggregate owns the business
    // rules for champions (e.g., a singles tournament can only have one champion, doubles
    // must have exactly two, etc.). These rules are part of the tournament's invariants.
    //
    // A "title" is a domain concept representing a bowler winning a tournament.
    // Stored as a collection of Bowler entities for EF Core navigation, but exposed
    // as BowlerIds through the ChampionIds property.
    private readonly List<Bowler> _champions = [];

    /// <summary>
    /// Gets the collection of bowler IDs for champions of this tournament.
    /// The tournament owns the business rules for how many champions are valid.
    /// </summary>
    public IReadOnlyCollection<BowlerId> ChampionIds => _champions.ConvertAll(b => b.Id);

    /// <summary>
    /// Internal navigation property to champion bowlers for EF Core many-to-many relationship.
    /// </summary>
    internal IReadOnlyCollection<Bowler> Champions
    {
        get => _champions;
        init => _champions = value?.ToList() ?? [];
    }

    internal Tournament()
        : base(TournamentId.New())
    {}

    /// <summary>
    /// Adds a champion to this tournament.
    /// </summary>
    /// <param name="bowler">The bowler entity to add as champion. The application service is responsible
    /// for loading this aggregate before calling this method.</param>
    /// <remarks>
    /// This method enforces tournament-specific invariants about champions. Cross-aggregate business rules
    /// (e.g., age requirements for senior tournaments) should be validated in domain services before calling.
    /// </remarks>
    public void AddChampion(Bowler bowler)
    {
        ArgumentNullException.ThrowIfNull(bowler);
        _champions.Add(bowler);
    }
}
