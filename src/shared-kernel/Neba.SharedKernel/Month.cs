using Ardalis.SmartEnum;

namespace Neba;

/// <summary>
/// Represents a month of the year.
/// </summary>
public sealed class Month
    : SmartEnum<Month>
{
    /// <summary>
    /// Represents the month of January.
    /// </summary>
    public static readonly Month January = new("January", 1);

    /// <summary>
    /// Represents the month of February.
    /// </summary>
    public static readonly Month February = new("February", 2);

    /// <summary>
    /// Represents the month of March.
    /// </summary>
    public static readonly Month March = new("March", 3);

    /// <summary>
    /// Represents the month of April.
    /// </summary>
    public static readonly Month April = new("April", 4);

    /// <summary>
    /// Represents the month of May.
    /// </summary>
    public static readonly Month May = new("May", 5);

    /// <summary>
    /// Represents the month of June.
    /// </summary>
    public static readonly Month June = new("June", 6);

    /// <summary>
    /// Represents the month of July.
    /// </summary>
    public static readonly Month July = new("July", 7);

    /// <summary>
    /// Represents the month of August.
    /// </summary>
    public static readonly Month August = new("August", 8);

    /// <summary>
    /// Represents the month of September.
    /// </summary>
    public static readonly Month September = new("September", 9);

    /// <summary>
    /// Represents the month of October.
    /// </summary>
    public static readonly Month October = new("October", 10);

    /// <summary>
    /// Represents the month of November.
    /// </summary>
    public static readonly Month November = new("November", 11);

    /// <summary>
    /// Represents the month of December.
    /// </summary>
    public static readonly Month December = new("December", 12);

    /// <summary>
    /// Initializes a new instance of the <see cref="Month"/> class.
    /// </summary>
    /// <param name="name">The full name of the month.</param>
    /// <param name="value">The numeric value of the month (1-12).</param>
    private Month(string name, int value)
        : base(name, value)
    {
    }

    /// <summary>
    /// Here for EF Core materialization purposes only.
    /// </summary>
    private Month()
        : base("", 0)
    { }

    /// <summary>
    /// Returns the three-letter abbreviation for the month (e.g., "Jan" for January).
    /// </summary>
    /// <returns>The three-letter abbreviated month name.</returns>
    public string ToShortString()
    {
        return this.Name.Substring(0, 3);
    }
}
