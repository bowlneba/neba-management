using System.Text.RegularExpressions;
using ErrorOr;

namespace Neba.Domain.Contact;

/// <summary>
/// Represents a phone number value object in the domain.
/// Encapsulates a country code, the digits of the number, and an optional extension.
/// </summary>
public sealed partial record PhoneNumber
{

    /// <summary>
    /// Gets an empty <see cref="PhoneNumber"/> instance with default (empty) values.
    /// Useful as a sentinel or default value where a non-null <see cref="PhoneNumber"/> is required.
    /// </summary>
    public static PhoneNumber Empty
        => new();

    /// <summary>
    /// Gets the ISO country calling code for the phone number (e.g. "1" for North America).
    /// </summary>
    public string CountryCode { get; internal init; } = string.Empty;

    /// <summary>
    /// Gets the digits-only phone number (no formatting characters).
    /// For North American numbers this will be 10 digits (NPA-NXX-XXXX).
    /// </summary>
    public string Number { get; internal init; } = string.Empty;

    /// <summary>
    /// Gets the optional phone extension (digits only) or <c>null</c> when none is present.
    /// </summary>
    public string? Extension { get; internal init; }

    /// <summary>
    /// Creates a <see cref="PhoneNumber"/> for North American Numbering Plan (NANP) numbers.
    /// </summary>
    /// <param name="phoneNumber">The input phone number string which may include formatting characters.</param>
    /// <param name="extension">An optional extension value which may include non-digit characters.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing a valid <see cref="PhoneNumber"/> on success;
    /// otherwise an error describing why the input was invalid.
    /// </returns>
    public static ErrorOr<PhoneNumber> CreateNorthAmerican(string phoneNumber, string? extension = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return PhoneNumberErrors.PhoneNumberIsRequired;
        }

        string digits = DigitsOnly().Replace(phoneNumber, string.Empty);

        if (digits.Length == 11 && digits[0] == '1')
        {
            digits = digits[1..];
        }

        if (digits.Length != 10)
        {
            return PhoneNumberErrors.InvalidNorthAmericanPhoneNumber(digits);
        }

        string areaCode = digits[..3];

        if (!IsValidNorthAmericanAreaCode(areaCode))
        {
            return PhoneNumberErrors.InvalidNorthAmericanAreaCode(areaCode);
        }

        string? cleanExtension = string.IsNullOrWhiteSpace(extension)
            ? null
            : DigitsOnly().Replace(extension!, string.Empty);

        return new PhoneNumber
        {
            CountryCode = "1",
            Number = digits,
            Extension = cleanExtension
        };
    }

    /// <summary>
    /// Regex generator that matches any non-digit character. Used to strip formatting.
    /// </summary>
    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnly();

    /// <summary>
    /// Validates the basic rules for a North American area code.
    /// Rules enforced:
    /// - First digit must be 2-9
    /// - Cannot be an N11 service code (e.g. 211, 311)
    /// </summary>
    /// <param name="areaCode">The 3-digit area code to validate.</param>
    /// <returns><c>true</c> if the area code is valid; otherwise <c>false</c>.</returns>
    private static bool IsValidNorthAmericanAreaCode(string areaCode)
    {
        // Basic rules:
        // - First digit 2-9
        // - Second digit 0-9
        // - Third digit 0-9
        // - Cannot be N11 (211, 311, etc.)

        if (areaCode.Length != 3) return false;

        char first = areaCode[0];
        char second = areaCode[1];
        char third = areaCode[2];

        if (first < '2' || first > '9') return false;
        if (second == '1' && third == '1') return false; // N11 codes

        return true;
    }

    /// <summary>
    /// Returns the phone number formatted in the standard North American style: (NPA) NXX-XXXX.
    /// </summary>
    /// <returns>A formatted phone number string like "(555) 123-4567".</returns>
    public string ToFormattedString()
    {
        // (555) 123-4567
        return $"({Number[..3]}) {Number[3..6]}-{Number[6..]}";
    }

    /// <summary>
    /// Returns the formatted phone number including the extension when present.
    /// </summary>
    /// <returns>
    /// The formatted phone number, optionally suffixed with " x{extension}" when an extension exists.
    /// </returns>
    public string ToFormattedStringWithExtension()
    {
        string formatted = ToFormattedString();
        return Extension is null ? formatted : $"{formatted} x{Extension}";
    }
}
