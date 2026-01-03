using Ardalis.SmartEnum;

namespace Neba.Domain.Tournaments;


/// <summary>
/// Represents a type of tournament, including its name, unique value, and team size.
/// Inherits from <see cref="SmartEnum{TournamentType}"/> for enhanced enum-like behavior.
/// </summary>
/// <remarks>
/// Use <see cref="TournamentType"/> to refer to a specific tournament format (e.g., Singles, Doubles).
/// </remarks>
public sealed class TournamentType
    : SmartEnum<TournamentType>
{
    /// <summary>
    /// Singles tournament (1 player per team).
    /// </summary>
    public static readonly TournamentType Singles = new("Singles", 10, 1, true);

    /// <summary>
    /// Doubles tournament (2 players per team).
    /// </summary>
    public static readonly TournamentType Doubles = new("Doubles", 20, 2, true);

    /// <summary>
    /// Trios tournament (3 players per team).
    /// </summary>
    public static readonly TournamentType Trios = new("Trios", 30, 3, true);

    /// <summary>
    /// Baker tournament (5 players per team).
    /// </summary>
    public static readonly TournamentType Baker = new("Baker", 50, 5, true);

    /// <summary>
    /// Non-Champions tournament.
    /// </summary>
    public static readonly TournamentType NonChampions = new("Non-Champions", 11, 1, true);

    /// <summary>
    /// Tournament of Champions event.
    /// </summary>
    public static readonly TournamentType TournamentOfChampions = new("Tournament of Champions", 12, 1, true);

    /// <summary>
    /// Invitational tournament.
    /// </summary>
    public static readonly TournamentType Invitational = new("Invitational", 13, 1, true);

    /// <summary>
    /// Masters tournament.
    /// </summary>
    public static readonly TournamentType Masters = new("Masters", 14, 1, true);

    /// <summary>
    /// High Roller tournament.
    /// </summary>
    public static readonly TournamentType HighRoller = new("High Roller", 15, 1, false);

    /// <summary>
    /// Senior tournament.
    /// </summary>
    public static readonly TournamentType Senior = new("Senior", 16, 1, true);

    /// <summary>
    /// Women tournament.
    /// </summary>
    public static readonly TournamentType Women = new("Women", 17, 1, true);

    /// <summary>
    /// Over 40 tournament.
    /// </summary>
    public static readonly TournamentType OverForty = new("Over 40", 18, 1, false);

    /// <summary>
    /// 40-49 age group tournament.
    /// </summary>
    public static readonly TournamentType FortyToFortyNine = new("40 - 49", 19, 1, false);

    /// <summary>
    /// Over/Under 50 Doubles tournament (2 players per team).
    /// </summary>
    public static readonly TournamentType OverUnderFiftyDoubles = new("Over/Under 50 Doubles", 21, 2, true);

    /// <summary>
    /// Over/Under 40 Doubles tournament (2 players per team).
    /// </summary>
    public static readonly TournamentType OverUnderFortyDoubles = new("Over/Under 40 Doubles", 22, 2, false);

    /// <summary>
    /// Initializes a new instance of the <see cref="TournamentType"/> class.
    /// </summary>
    /// <param name="name">The display name of the tournament type.</param>
    /// <param name="value">The unique integer value for the tournament type.</param>
    /// <param name="teamSize">The number of players per team for this tournament type.</param>
    /// <param name="activeFormat">Indicates whether this tournament type is an active format.</param>
    private TournamentType(string name, int value, int teamSize, bool activeFormat)
        : base(name, value)
    {
        TeamSize = teamSize;
        ActiveFormat = activeFormat;
    }

    /// <summary>
    /// Here for EF Core materialization purposes only.
    /// </summary>
    private TournamentType()
        : base("", 0)
    { }

    /// <summary>
    /// Gets the number of players per team for this tournament type.
    /// </summary>
    public int TeamSize { get; }

    /// <summary>
    /// Indicates whether this tournament type is an active format.
    /// </summary>
    /// <value>True if the tournament type is an active format; otherwise, false.</value>
    public bool ActiveFormat { get; }
}
