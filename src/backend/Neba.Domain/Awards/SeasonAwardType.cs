using Ardalis.SmartEnum;

namespace Neba.Domain.Awards;

/// <summary>
/// Represents the types of season awards that can be given in NEBA, using a SmartEnum pattern for extensibility.
/// </summary>
public sealed class SeasonAwardType
    : SmartEnum<SeasonAwardType>
{
    internal static readonly SeasonAwardType s_default = new("Default", 0);

    /// <summary>
    /// Awarded to the bowler with the best overall performance during the season.
    /// </summary>
    public static readonly SeasonAwardType BowlerOfTheYear = new("Bowler of the Year", 1);

    /// <summary>
    /// Awarded for achieving the highest average during the season.
    /// </summary>
    public static readonly SeasonAwardType HighAverage = new("High Average", 2);

    /// <summary>
    /// Awarded for the highest 5-game block score in a single event or season.
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
