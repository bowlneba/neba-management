using Ardalis.SmartEnum;

namespace Neba.Domain.Addresses;

/// <summary>
/// Represents a country with its name and ISO 3166-1 alpha-2 code.
/// </summary>
public sealed class Country
    : SmartEnum<Country, string>
{
    /// <summary>United States (US).</summary>
    public static readonly Country UnitedStates = new("United States", "US");

    /// <summary>Canada (CA).</summary>
    public static readonly Country Canada = new("Canada", "CA");

    private Country(string name, string value)
        : base(name, value)
    { }
}
