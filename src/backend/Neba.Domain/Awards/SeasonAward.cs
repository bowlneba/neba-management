using Neba.Domain.Abstractions;
using Neba.Domain.Bowlers;
using Neba.Domain.Bowlers.BowlerAwards;

namespace Neba.Domain.Awards;

/// <summary>
/// Represents a season award given to a bowler, including award type, season, and related statistics.
/// </summary>
public sealed class SeasonAward
    : Entity<SeasonAwardId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonAward"/> class with a new unique identifier.
    /// </summary>
    public SeasonAward()
        : base(SeasonAwardId.New())
    {
        AwardType = SeasonAwardType.s_default;
    }

    /// <summary>
    /// The type of season award being given.
    /// </summary>
    internal SeasonAwardType AwardType { get; init; }

    /// <summary>
    /// The season (e.g., "2025") for which the award is given.
    /// </summary>
    public required string Season { get; init; }

    /// <summary>
    /// The unique identifier of the bowler receiving the award.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// The bowler entity associated with the award. Internal use only.
    /// </summary>
    internal Bowler Bowler { get; } = null!;

    /// <summary>
    /// The category for Bowler of the Year awards, if applicable.
    /// </summary>
    public BowlerOfTheYearCategory? BowlerOfTheYearCategory { get; init; }

    /// <summary>
    /// The highest block score achieved by the bowler in the season, if applicable.
    /// </summary>
    public int? HighBlockScore { get; init; }

    /// <summary>
    /// The total number of pins scored by the bowler in the season, if applicable.
    /// </summary>
    public int? SeasonTotalPins { get; init; }

    /// <summary>
    /// The total number of games played by the bowler in the season, if applicable.
    /// </summary>
    public int? SeasonTotalGames { get; init; }
}
