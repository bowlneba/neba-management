using System.Globalization;
using System.Text;
using ErrorOr;

namespace Neba.Domain.Bowlers;

/// <summary>
/// Represents a bowler's full legal and display name, including first name, last name, optional middle initial, suffix, and nickname, as defined in the NEBA domain ubiquitous language.
/// </summary>
/// <value>Bowler's name value object</value>
public sealed record Name
{
    /// <summary>
    /// The bowler's given first name, as used in official records and communications.
    /// </summary>
    /// <value>First name of the bowler</value>
    public required string FirstName { get; init; }

    /// <summary>
    /// The bowler's family or surname, as used in official records and communications.
    /// </summary>
    /// <value>Last name of the bowler</value>
    public required string LastName { get; init; }

    /// <summary>
    /// The bowler's middle initial, if provided, as used in legal or formal contexts.
    /// </summary>
    /// <value>Middle initial of the bowler</value>
    public string? MiddleInitial { get; init; }

    /// <summary>
    /// The suffix for the bowler's name (e.g., Jr., Sr., III), if applicable, as used in official records.
    /// </summary>
    /// <value>Suffix of the bowler's name</value>
    public string? Suffix { get; init; }

    /// <summary>
    /// The bowler's preferred nickname, used for informal display or communications.
    /// </summary>
    /// <value>Nickname of the bowler</value>
    public string? Nickname { get; init; }

    internal Name()
    { }

    /// <summary>
    /// Creates a new <see cref="Name"/> value object after validating required fields according to NEBA domain rules.
    /// </summary>
    /// <param name="firstName">The bowler's first name (required).</param>
    /// <param name="lastName">The bowler's last name (required).</param>
    /// <param name="middleInitial">The bowler's middle initial (optional).</param>
    /// <param name="suffix">The bowler's name suffix (optional).</param>
    /// <param name="nickname">The bowler's nickname (optional).</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing the created <see cref="Name"/> or validation errors.</returns>
    public static ErrorOr<Name> Create(
        string firstName,
        string lastName,
        string? middleInitial = null,
        string? suffix = null,
        string? nickname = null
    )
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add(NameErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            errors.Add(NameErrors.LastNameRequired);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Name
        {
            FirstName = firstName,
            LastName = lastName,
            MiddleInitial = middleInitial,
            Suffix = suffix,
            Nickname = nickname
        };
    }

    /// <summary>
    /// Returns the bowler's legal name in the format: FirstName [MiddleInitial.] LastName[, Suffix.]
    /// </summary>
    /// <returns>The legal name string for the bowler.</returns>
    public string ToLegalName()
    {
        StringBuilder parts = new(FirstName);

        if (!string.IsNullOrWhiteSpace(MiddleInitial))
        {
            parts.Append(CultureInfo.CurrentCulture, $" {MiddleInitial}.");
        }

        parts.Append(CultureInfo.CurrentCulture, $" {LastName}");

        if (!string.IsNullOrWhiteSpace(Suffix))
        {
            parts.Append(CultureInfo.CurrentCulture, $", {Suffix}");
        }

        return parts.ToString();
    }

    /// <summary>
    /// Returns the bowler's display name, using the nickname if available, otherwise the first and last name.
    /// </summary>
    /// <returns>The display name string for the bowler.</returns>
    public string ToDisplayName()
        => !string.IsNullOrWhiteSpace(Nickname)
            ? $"{Nickname} {LastName}"
            : $"{FirstName} {LastName}";

    /// <summary>
    /// Returns the bowler's formal name in the format: FirstName LastName.
    /// </summary>
    /// <returns>The formal name string for the bowler.</returns>
    public string ToFormalName()
        => $"{FirstName} {LastName}";

    ///<inheritdoc />
    public override string ToString()
        => ToLegalName();
}
