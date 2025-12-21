using ErrorOr;

namespace Neba.Domain;

internal static class NameErrors
{
    public static readonly Error FirstNameRequired
        = Error.Validation(
            code: "Name.FirstName.Required",
            description: "First name is required."
        );

    public static readonly Error LastNameRequired
        = Error.Validation(
            code: "Name.LastName.Required",
            description: "Last name is required."
        );
}
