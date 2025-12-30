using ErrorOr;

namespace Neba.Domain.Geography;

internal static class DistanceCalculatorErrors
{
    public static readonly Error AddressMissingCoordinates =
        Error.Validation(
            code: "Address.DistanceCalculator.AddressMissingCoordinates",
            description: "One or both addresses are missing coordinates.");
}
