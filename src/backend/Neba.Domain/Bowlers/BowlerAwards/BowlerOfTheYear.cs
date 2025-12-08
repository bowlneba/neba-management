using Neba.Domain.Abstractions;
using Neba.Domain.Awards;

namespace Neba.Domain.Bowlers.BowlerAwards;

/// <summary>
/// Represents a Bowler of the Year award for a specific season and category.
/// </summary>
public sealed class BowlerOfTheYear
    : Entity<BowlerOfTheYearId>
{
    /// <summary>
    /// EF Core parameterless constructor for ORM support.
    /// </summary>
    public BowlerOfTheYear()
        : base(BowlerOfTheYearId.New())
    { }

    /// <summary>
    /// The season in which the award was given (e.g., "2024-2025").
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// The category of the Bowler of the Year award (e.g., Open, Woman, Senior).
    /// </summary>
    public required BowlerOfTheYearCategory Category { get; init; }

    /// <summary>
    /// The unique identifier of the bowler who received the award.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// The bowler entity associated with this award. Internal for ORM navigation.
    /// </summary>
    internal Bowler Bowler { get; } = null!;
}
