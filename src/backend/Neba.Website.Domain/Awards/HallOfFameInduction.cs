
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Bowlers;

namespace Neba.Website.Domain.Awards;

/// <summary>
/// Represents an induction into the Hall of Fame.
/// </summary>
/// <remarks>
/// This aggregate is the domain root for a Hall of Fame entry and
/// uses a strongly-typed <see cref="HallOfFameId"/> (ULID) as its identifier.
/// Keep business logic for induction events and rules on this aggregate.
/// </remarks>
public sealed class HallOfFameInduction
    : Aggregate<HallOfFameId>
{
    /// <summary>
    /// Initializes a new instance of <see cref="HallOfFameInduction"/>
    /// with a generated <see cref="HallOfFameId"/>.
    /// </summary>
    public HallOfFameInduction()
        : base(HallOfFameId.New())
    { }

    /// <summary>
    /// The year the induction took place.
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// The inducted <see cref="Bowler"/> for this Hall of Fame entry.
    /// </summary>
    internal Bowler Bowler { get; init; } = null!;

    /// <summary>
    /// The categories associated with this induction.
    /// </summary>
    public IReadOnlyCollection<HallOfFameCategory> Categories { get; init; } = [];
}
