using ErrorOr;

namespace Neba.Domain.Addresses;

internal static class AddressErrors
{
    public static readonly Error StreetIsRequired =
        Error.Validation(
            code: "Address.StreetIsRequired",
            description: "The street address is required.");

    public static readonly Error CityIsRequired =
        Error.Validation(
            code: "Address.CityIsRequired",
            description: "The city is required.");

    public static readonly Error PostalCodeIsRequired =
        Error.Validation(
            code: "Address.PostalCodeIsRequired",
            description: "The postal code is required.");

    public static Error InvalidPostalCode(string invalidPostalCode)
        => Error.Validation(
            code: "Address.InvalidPostalCode",
            description: "The postal code is not valid.",
            metadata: new Dictionary<string, object>
            {
                { "InvalidPostalCode", invalidPostalCode }
            });
}
