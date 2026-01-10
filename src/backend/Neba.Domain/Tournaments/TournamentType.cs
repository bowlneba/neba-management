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
    public static readonly TournamentType Singles = new(nameof(Singles), 100, 1, true);

    /// <summary>
    /// Doubles tournament (2 players per team).
    /// </summary>
    public static readonly TournamentType Doubles = new(nameof(Doubles), 200, 2, true);

    /// <summary>
    /// Trios tournament (3 players per team).
    /// </summary>
    public static readonly TournamentType Trios = new(nameof(Trios), 300, 3, true);

    /// <summary>
    /// Baker tournament (5 players per team).
    /// </summary>
    public static readonly TournamentType Baker = new(nameof(Baker), 500, 5, true);

    /// <summary>
    /// Non-Champions tournament.
    /// </summary>
    public static readonly TournamentType NonChampions = new("Non-Champions", 101, 1, true);

    /// <summary>
    /// Tournament of Champions event.
    /// </summary>
    public static readonly TournamentType TournamentOfChampions = new("Tournament of Champions", 102, 1, true);

    /// <summary>
    /// Invitational tournament.
    /// </summary>
    public static readonly TournamentType Invitational = new(nameof(Invitational), 103, 1, true);

    /// <summary>
    /// Masters tournament.
    /// </summary>
    public static readonly TournamentType Masters = new(nameof(Masters), 104, 1, true);

    /// <summary>
    /// High Roller tournament.
    /// </summary>
    public static readonly TournamentType HighRoller = new("High Roller", 105, 1, false);

    /// <summary>
    /// Senior tournament.
    /// </summary>
    public static readonly TournamentType Senior = new(nameof(Senior), 106, 1, true);

    /// <summary>
    /// Women tournament.
    /// </summary>
    public static readonly TournamentType Women = new(nameof(Women), 107, 1, true);

    /// <summary>
    /// Over 40 tournament.
    /// </summary>
    public static readonly TournamentType OverForty = new("Over 40", 108, 1, false);

    /// <summary>
    /// 40-49 age group tournament.
    /// </summary>
    public static readonly TournamentType FortyToFortyNine = new("40 - 49", 109, 1, false);

    /// <summary>
    /// Youth tournament.
    /// </summary>
    public static readonly TournamentType Youth = new(nameof(Youth), 110, 1, true);

    /// <summary>
    /// Eliminator tournament.
    /// </summary>
    public static readonly TournamentType Eliminator = new(nameof(Eliminator), 111, 1, false);

    /// <summary>
    /// Over/Under 50 Doubles tournament (2 players per team).
    /// </summary>
    public static readonly TournamentType OverUnderFiftyDoubles = new("Over/Under 50 Doubles", 201, 2, true);

    /// <summary>
    /// Over/Under 40 Doubles tournament (2 players per team).
    /// </summary>
    public static readonly TournamentType OverUnderFortyDoubles = new("Over/Under 40 Doubles", 202, 2, false);

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
