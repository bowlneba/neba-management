using ErrorOr;

namespace Neba.Domain.Addresses;

internal static class AddressDistanceCalculatorErrors
{
    public static readonly Error AddressMissingCoordinates =
        Error.Validation(
            code: "Address.DistanceCalculator.AddressMissingCoordinates",
            description: "One or both addresses are missing coordinates.");
}
