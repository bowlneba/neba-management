namespace Neba.Domain.Awards;

public sealed class SeasonalAwardType
    : SmartEnum<SeasonalAwardType>
{
    public static readonly SeasonalAwardType BowlerOfTheYear = new("Bowler of the Year", 1);
    public static readonly SeasonalAwardType HighAverage = new("High Average", 2);
    public static readonly SeasonalAwardType High5GameBlock = new("High 5-Game Block", 3);

    private SeasonalAwardType(string name, int value)
        : base(name, value)
    { }

    private SeasonalAwardType()
        : this(string.Empty, 0)
    { }
}
