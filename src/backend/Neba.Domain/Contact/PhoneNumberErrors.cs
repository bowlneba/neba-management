using ErrorOr;

namespace Neba.Domain.Contact;

internal static class PhoneNumberErrors
{
    public static readonly Error PhoneNumberIsRequired =
        Error.Validation(
            code: "PhoneNumber.PhoneNumberIsRequired",
            description: "Phone number is required.");

    public static Error InvalidNorthAmericanPhoneNumber(string providedNumber)
        => Error.Validation(
            code: "PhoneNumber.InvalidNorthAmericanPhoneNumber",
            description: $"The provided phone number is not a valid North American phone number.",
            metadata: new Dictionary<string, object>
            {
                { "InvalidPhoneNumber", providedNumber }
            });

    public static Error InvalidNorthAmericanAreaCode(string areaCode)
        => Error.Validation(
            code: "PhoneNumber.InvalidNorthAmericanAreaCode",
            description: $"The area code is not a valid North American area code.",
            metadata: new Dictionary<string, object>
            {
                { "InvalidAreaCode", areaCode }
            });
}
