using Ardalis.SmartEnum;

namespace Neba.Domain;

/// <summary>
/// Represents one or more Hall of Fame categories for a person or entity.
/// </summary>
/// <remarks>
/// This type is a set of bit flags (powers of two) so multiple categories
/// can be combined with a bitwise OR (for example,
/// <c>SuperiorPerformance | FriendOfNeba</c>). Use <see cref="SmartFlagEnum{T}"/>
/// helpers or bitwise operations to test membership.
/// </remarks>
public sealed class HallOfFameCategory
    : SmartFlagEnum<HallOfFameCategory>
{
    /// <summary>
    /// No categories set.
    /// Use this value to represent the absence of any Hall of Fame category.
    /// </summary>
    public static readonly HallOfFameCategory None = new(nameof(None), 0);

    /// <summary>
    /// Indicates superior performance recognition.
    /// This flag has the value <c>1</c> (bit 0).
    /// </summary>
    public static readonly HallOfFameCategory SuperiorPerformance = new(nameof(SuperiorPerformance), 1 << 0);

    /// <summary>
    /// Indicates meritorious service recognition.
    /// This flag has the value <c>2</c> (bit 1).
    /// </summary>
    public static readonly HallOfFameCategory MeritoriousService = new(nameof(MeritoriousService), 1 << 1);

    /// <summary>
    /// Indicates a friend of NEBA recognition.
    /// This flag has the value <c>4</c> (bit 2).
    /// </summary>
    public static readonly HallOfFameCategory FriendOfNeba = new(nameof(FriendOfNeba), 1 << 2);

    private HallOfFameCategory(string name, int value)
        : base(name, value)
    { }
}
