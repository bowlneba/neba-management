using Ardalis.SmartEnum;

namespace Neba.Domain.Tournaments;

/// <summary>
/// Represents the ratio of a bowling pattern, including its name, unique value, and ratio range
/// </summary>
public sealed class PatternRatio
    : SmartEnum<PatternRatio>
{
    /// <summary>
    /// Sport ratio (less than 4.0).
    /// </summary>
    public static readonly PatternRatio Sport = new(nameof(Sport), 1, null, 4m);

    /// <summary>
    /// Challenge ratio (4.0 to 8.0).
    /// </summary>
    public static readonly PatternRatio Challenge = new(nameof(Challenge), 2, 4m, 8m);

    /// <summary>
    /// Recreation ratio (8.0 or more).
    /// </summary>
    public static readonly PatternRatio Recreation = new(nameof(Recreation), 3, 8m, null);

    private PatternRatio(
        string name,
        int value,
        decimal? minimumRatio,
        decimal? maximumRatio)
        : base(name, value)
    {
        MinimumRatio = minimumRatio;
        MaximumRatio = maximumRatio;
    }

    /// <summary>
    /// Gets the minimum ratio, if applicable.
    /// </summary>
    /// <value>The minimum ratio, or null if not applicable.</value>
    public decimal? MinimumRatio { get; }

    /// <summary>
    /// Gets the maximum ratio, if applicable.
    /// </summary>
    /// <value>The maximum ratio, or null if not applicable.</value>
    public decimal? MaximumRatio { get; }
}
