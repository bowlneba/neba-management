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

    internal Tournament()
        : base(TournamentId.New())
    {}

}
