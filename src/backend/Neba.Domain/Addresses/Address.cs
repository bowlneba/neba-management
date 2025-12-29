using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ardalis.SmartEnum.SystemTextJson;
using ErrorOr;

namespace Neba.Domain.Addresses;

/// <summary>
/// Represents a postal address, including street, city, region, country, postal code, and optional coordinates.
/// </summary>
public sealed partial record Address
{
    /// <summary>
    /// Gets the street address (e.g., house number and street name).
    /// </summary>
    public string Street { get; internal init; } = string.Empty;

    /// <summary>
    /// Gets the unit, apartment, or suite number (optional).
    /// </summary>
    public string? Unit { get; internal init; }

    /// <summary>
    /// Gets the city or locality.
    /// </summary>
    public string City { get; internal init; } = string.Empty;

    /// <summary>
    /// Gets the region, state, or province.
    /// </summary>
    public string Region { get; internal init; } = string.Empty;

    /// <summary>
    /// Gets the country for the address.
    /// </summary>
    [JsonConverter(typeof(SmartEnumValueConverter<Country, string>))]
    public Country Country { get; internal init; } = Country.s_default;

    /// <summary>
    /// Gets the postal or ZIP code.
    /// </summary>
    public string PostalCode { get; internal init; } = string.Empty;

    /// <summary>
    /// Gets the geographic coordinates (latitude/longitude) for the address, if available.
    /// </summary>
    public Coordinates? Coordinates { get; internal init; }

    /// <summary>
    /// Gets an empty <see cref="Address"/> instance.
    /// </summary>
    public static Address Empty { get; } = new Address();

    /// <summary>
    /// Creates a new <see cref="Address"/> for a United States address.
    /// </summary>
    /// <param name="street">Street address (required).</param>
    /// <param name="unit">Unit, apartment, or suite number (optional).</param>
    /// <param name="city">City or locality (required).</param>
    /// <param name="state">A <see cref="UsState"/> representing the state (required).</param>
    /// <param name="zipCode">ZIP code. Must match US ZIP code formats (5 digits or 5+4).</param>
    /// <param name="coordinates">Optional geographic coordinates for the address.</param>
    /// <returns>
    /// An <see cref="ErrorOr{Address}"/> containing the created <see cref="Address"/> when input
    /// is valid, or an error from <see cref="AddressErrors"/> describing the first validation failure.
    /// </returns>
    public static ErrorOr<Address> Create(
        string street,
        string? unit,
        string city,
        UsState state,
        string zipCode,
        Coordinates? coordinates = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (string.IsNullOrWhiteSpace(street))
        {
            return AddressErrors.StreetIsRequired;
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return AddressErrors.CityIsRequired;
        }

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            return AddressErrors.PostalCodeIsRequired;
        }

        if (!IsValidZipCode(zipCode))
        {
            return AddressErrors.InvalidPostalCode(zipCode);
        }

        return new Address
        {
            Street = street,
            Unit = unit,
            City = city,
            Region = state.Value,
            Country = Country.UnitedStates,
            PostalCode = NormalizeZipCode(zipCode),
            Coordinates = coordinates
        };
    }

    /// <summary>
    /// Creates a new <see cref="Address"/> for a Canadian address.
    /// </summary>
    /// <param name="street">Street address (required).</param>
    /// <param name="unit">Unit, apartment, or suite number (optional).</param>
    /// <param name="city">City or locality (required).</param>
    /// <param name="province">A <see cref="CanadianProvince"/> representing the province or territory (required).</param>
    /// <param name="postalCode">Postal code. Must match Canadian postal code format (A1A 1A1 or A1A1A1).</param>
    /// <param name="coordinates">Optional geographic coordinates for the address.</param>
    /// <returns>
    /// An <see cref="ErrorOr{Address}"/> containing the created <see cref="Address"/> when input
    /// is valid, or an error from <see cref="AddressErrors"/> describing the first validation failure.
    /// The returned address will have the postal code normalized to upper-case.
    /// </returns>
    public static ErrorOr<Address> Create(
        string street,
        string? unit,
        string city,
        CanadianProvince province,
        string postalCode,
        Coordinates? coordinates = null)
    {
        ArgumentNullException.ThrowIfNull(province);

        if (string.IsNullOrWhiteSpace(street))
        {
            return AddressErrors.StreetIsRequired;
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return AddressErrors.CityIsRequired;
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return AddressErrors.PostalCodeIsRequired;
        }

        if (!IsValidCanadianPostalCode(postalCode))
        {
            return AddressErrors.InvalidPostalCode(postalCode);
        }

        return new Address
        {
            Street = street,
            Unit = unit,
            City = city,
            Region = province.Value,
            Country = Country.Canada,
            PostalCode = NormalizeCanadianPostalCode(postalCode),
            Coordinates = coordinates
        };
    }

    private static bool IsValidZipCode(string zipCode)
    {
        // Simple US ZIP code validation: 5 digits or 5 digits + 4 digits
        return ZipCodeRegex().IsMatch(zipCode);
    }

    private static string NormalizeZipCode(string zipCode)
    {
        // Remove any existing dash
        string digitsOnly = zipCode.Replace("-", string.Empty, StringComparison.Ordinal);

        // If 9 digits, format as 12345-6789
        // If 5 digits, return as-is
        return digitsOnly.Length == 9
            ? $"{digitsOnly[..5]}-{digitsOnly[5..]}"
            : digitsOnly;
    }

    [GeneratedRegex(@"^\d{5}(-?\d{4})?$")]
    private static partial Regex ZipCodeRegex();

    private static bool IsValidCanadianPostalCode(string postalCode)
    {
        // Simple Canadian postal code validation: A1A 1A1 or A1A1A1
        return CanadianPostalCodeRegex().IsMatch(postalCode);
    }

    private static string NormalizeCanadianPostalCode(string postalCode)
    {
        // Remove any existing spaces and convert to uppercase
        string normalized = postalCode.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

        // Insert space after 3rd character: A1A 1A1
        return $"{normalized[..3]} {normalized[3..]}";
    }

    [GeneratedRegex(@"^[ABCEGHJ-NPRSTVXY]\d[ABCEGHJ-NPRSTV-Z] ?\d[ABCEGHJ-NPRSTV-Z]\d$",
    RegexOptions.IgnoreCase)]
    private static partial Regex CanadianPostalCodeRegex();
}
