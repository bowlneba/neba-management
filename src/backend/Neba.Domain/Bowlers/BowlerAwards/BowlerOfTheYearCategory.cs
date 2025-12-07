using Ardalis.SmartEnum;

namespace Neba.Domain.Bowlers.BowlerAwards;

/// <summary>
/// Represents a category for the Bowler of the Year award, using a smart enum pattern.
/// </summary>
public sealed class BowlerOfTheYearCategory
    : SmartEnum<BowlerOfTheYearCategory>
{
    /// <summary>
    /// Open category for Bowler of the Year.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Open = new("Bowler of the Year", 1);

    /// <summary>
    /// Woman category for Bowler of the Year.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Woman = new("Woman Bowler of the Year", 2);

    /// <summary>
    /// Senior category for Bowler of the Year.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Senior = new("Senior Bowler of the Year", 50);

    /// <summary>
    /// Super Senior category for Bowler of the Year.
    /// </summary>
    public static readonly BowlerOfTheYearCategory SuperSenior = new("Super Senior Bowler of the Year", 60);

    /// <summary>
    /// Rookie of the Year category.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Rookie = new("Rookie of the Year", 10);

    /// <summary>
    /// Youth category for Bowler of the Year.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Youth = new("Youth Bowler of the Year", 20);

    /// <summary>
    /// Initializes a new instance of the <see cref="BowlerOfTheYearCategory"/> class with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the category.</param>
    /// <param name="value">The integer value of the category.</param>
    private BowlerOfTheYearCategory(string name, int value)
        : base(name, value)
    { }

    /// <summary>
    /// Private parameterless constructor for serialization or ORM support.
    /// </summary>
    private BowlerOfTheYearCategory()
        : this(string.Empty, 0)
    {}
}
