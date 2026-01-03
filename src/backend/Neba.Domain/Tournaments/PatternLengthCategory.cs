using Ardalis.SmartEnum;

namespace Neba.Domain.Tournaments;

/// <summary>
/// Represents the length of a bowling pattern, including its name, unique value, and length range
/// in feet.
/// Inherits from <see cref="SmartEnum{PatternLengthCategory}"/> for enhanced enum-like behavior.
/// </summary>
public sealed class PatternLengthCategory
    : SmartEnum<PatternLengthCategory>
{
    /// <summary>
    /// Short pattern (less than 38 feet).
    /// </summary>
    public static readonly PatternLengthCategory ShortPattern = new(nameof(ShortPattern), 1, null, 37);

    /// <summary>
    /// Medium pattern (38 to 42 feet).
    /// </summary>
    public static readonly PatternLengthCategory MediumPattern = new(nameof(MediumPattern), 2, 38, 42);

    /// <summary>
    /// Long pattern (43 feet or more).
    /// </summary>
    public static readonly PatternLengthCategory LongPattern = new(nameof(LongPattern), 3, 43, null);

    private PatternLengthCategory(
        string name,
        int value,
        int? minimumLength,
        int? maximumLength)
        : base(name, value)
    {
        MinimumLength = minimumLength;
        MaximumLength = maximumLength;
    }

    /// <summary>
    /// Gets the minimum length of the pattern in feet, if applicable.
    /// </summary>
    /// <value>The minimum length of the pattern in feet, or null if not applicable.</value>
    public int? MinimumLength { get; }

    /// <summary>
    /// Gets the maximum length of the pattern in feet, if applicable.
    /// </summary>
    /// <value>The maximum length of the pattern in feet, or null if not applicable.</value>
    public int? MaximumLength { get; }
}
