using Ardalis.SmartEnum;

namespace Neba.Website.Domain.Awards;

/// <summary>
/// The types of season awards that can be earned by NEBA bowlers, each recognizing different aspects of competitive excellence.
/// Uses SmartEnum pattern for extensibility.
/// </summary>
public sealed class SeasonAwardType
    : SmartEnum<SeasonAwardType>
{
    internal static readonly SeasonAwardType s_default = new("default", 0);

    /// <summary>
    /// Awarded to the bowler(s) with the best overall performance during the season, determined by a points system
    /// based on tournament finishes. Points are earned through placement in stat-eligible tournaments, with additional
    /// points awarded for being high qualifier. Different tournament types have different point structures.
    /// </summary>
    public static readonly SeasonAwardType BowlerOfTheYear = new("Bowler of the Year", 1);

    /// <summary>
    /// Awarded for achieving the highest average score during the season across stat-eligible tournaments.
    /// Eligibility requires a minimum of 4.5 × (number of stat-eligible tournaments completed) games, with decimals dropped.
    /// All games bowled in stat-eligible tournaments count toward the average (exception: baker team finals do not count).
    /// </summary>
    public static readonly SeasonAwardType HighAverage = new("High Average", 2);

    /// <summary>
    /// Awarded for the highest total score across the first 5 qualifying games in a single tournament.
    /// No minimum tournament requirement beyond the one tournament where the score was achieved.
    /// The "5Game" designation preserves historical context; future changes would create new award types.
    /// </summary>
    public static readonly SeasonAwardType High5GameBlock = new("High 5-Game Block", 3);

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonAwardType"/> class with the specified name and value.
    /// </summary>
    /// <param name="name">The display name of the award type.</param>
    /// <param name="value">The unique value of the award type.</param>
    private SeasonAwardType(string name, int value)
        : base(name, value)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonAwardType"/> class with default values.
    /// </summary>
    private SeasonAwardType()
        : this(string.Empty, 0)
    { }
}
