using Ardalis.SmartEnum;

namespace Neba.Domain.Awards;

/// <summary>
/// Categories used to group bowlers for Bowler of the Year awards, allowing recognition across different
/// demographics and experience levels. Each category has specific eligibility criteria, and bowlers can
/// compete in multiple categories simultaneously.
/// </summary>
public sealed class BowlerOfTheYearCategory
    : SmartEnum<BowlerOfTheYearCategory>
{
    /// <summary>
    /// Open category for Bowler of the Year - all NEBA members are eligible.
    /// This is the most prestigious BOTY category with no restrictions.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Open = new("Bowler of the Year", 1);

    /// <summary>
    /// Woman Bowler of the Year - female NEBA members competing in stat-eligible tournaments open to all women.
    /// Women bowling in tournaments restricted to other categories do not earn Woman BOTY points for those events.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Woman = new("Woman Bowler of the Year", 2);

    /// <summary>
    /// Senior Bowler of the Year - bowlers aged 50 or older (age determined as of each tournament date).
    /// Example: A bowler turning 50 on June 30 can earn Senior BOTY points starting July 1.
    /// </summary>
    public static readonly BowlerOfTheYearCategory Senior = new("Senior Bowler of the Year", 50);

    /// <summary>
    /// Super Senior Bowler of the Year - bowlers aged 60 or older (age determined as of each tournament date).
    /// Same age determination rules as Senior category.
    /// </summary>
    public static readonly BowlerOfTheYearCategory SuperSenior = new("Super Senior Bowler of the Year", 60);

    /// <summary>
    /// Rookie of the Year - bowlers in their first year of NEBA membership.
    /// Eligibility is based on the season in which they purchased a "New Membership" (not a "Renewal Membership").
    /// </summary>
    public static readonly BowlerOfTheYearCategory Rookie = new("Rookie of the Year", 10);

    /// <summary>
    /// Youth Bowler of the Year - bowlers under age 18 (age determined as of each tournament date).
    /// Example: A bowler turning 18 on June 30 can earn Youth BOTY points through June 29 only.
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
    public BowlerOfTheYearCategory()
        : this("default", 0)
    { }
}
