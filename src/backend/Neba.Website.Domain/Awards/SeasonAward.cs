using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Bowlers;

namespace Neba.Website.Domain.Awards;

/// <summary>
/// A season award recognizes exceptional achievement by a bowler during a NEBA bowling season.
/// Awards are given for outstanding performance in specific categories: overall performance (Bowler of the Year),
/// highest average, or highest 5-game qualifying block score.
/// </summary>
/// <remarks>
/// Season awards can be corrected if errors are discovered, though this has not yet occurred in practice.
/// A bowler can win multiple awards in the same season if they meet eligibility criteria for each.
/// </remarks>
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
    /// The type of season award (BowlerOfTheYear, HighAverage, or High5GameBlock).
    /// </summary>
    internal SeasonAwardType AwardType { get; init; }

    /// <summary>
    /// The season for which the award is given (typically "YYYY" format, e.g., "2025").
    /// </summary>
    /// <remarks>
    /// Standard seasons run January 1 - December 31 (calendar year).
    /// Exception: "2020/2021" was a combined season due to COVID-19 tournament cancellations.
    /// The Season property is a string to accommodate the "2020/2021" format.
    /// </remarks>
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
    /// The category for Bowler of the Year awards (Open, Woman, Senior, SuperSenior, Rookie, Youth).
    /// Null for non-BOTY awards (HighAverage, High5GameBlock).
    /// </summary>
    public BowlerOfTheYearCategory? BowlerOfTheYearCategory { get; init; }

    /// <summary>
    /// The highest 5-game block score (for High5GameBlock awards only).
    /// Null for other award types.
    /// </summary>
    public int? HighBlockScore { get; init; }

    /// <summary>
    /// The bowler's season average (for HighAverage awards only).
    /// Null for other award types.
    /// </summary>
    public decimal? Average { get; init; }

    /// <summary>
    /// Total games bowled in stat-eligible tournaments during the season (for HighAverage awards).
    /// Provides context for comparison between high average winners.
    /// Null for other award types.
    /// </summary>
    public int? SeasonTotalGames { get; init; }

    /// <summary>
    /// Number of tournaments participated in during the season (for HighAverage awards).
    /// Provides context for comparison and potential tie-breaking.
    /// Null for other award types.
    /// </summary>
    public int? Tournaments { get; init; }
}
