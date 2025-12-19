using System.Globalization;
using System.Text;
using ErrorOr;

namespace Neba.Domain;

/// <summary>
/// A value object representing a bowler's complete name information, including legal name components
/// and an optional nickname for informal display. Provides multiple formatting options for different
/// contexts (legal documents, public display, formal communications).
/// </summary>
public sealed record Name
{
    /// <summary>
    /// The bowler's given first name, used in official records.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// The bowler's family or surname, used in official records.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// The bowler's middle name (optional), used in legal or formal contexts.
    /// </summary>
    public string? MiddleName { get; init; }

    /// <summary>
    /// Name suffix such as Jr., Sr., III, etc. (optional), used in official records.
    /// </summary>
    public string? Suffix { get; init; }

    /// <summary>
    /// The bowler's preferred informal name (optional).
    /// Can be traditional derivatives (e.g., "Dave" for David, "Mike" for Michael) or completely unrelated nicknames.
    /// </summary>
    public string? Nickname { get; init; }

    internal Name()
    { }

    /// <summary>
    /// Creates a new <see cref="Name"/> value object after validating required fields.
    /// </summary>
    /// <param name="firstName">The bowler's first name (required - cannot be null or whitespace).</param>
    /// <param name="lastName">The bowler's last name (required - cannot be null or whitespace).</param>
    /// <param name="middleName">The bowler's middle name (optional).</param>
    /// <param name="suffix">The bowler's name suffix (optional).</param>
    /// <param name="nickname">The bowler's nickname (optional - no restrictions on content).</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing the created <see cref="Name"/> or validation errors.</returns>
    public static ErrorOr<Name> Create(
        string firstName,
        string lastName,
        string? middleName = null,
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
            MiddleName = middleName,
            Suffix = suffix,
            Nickname = nickname
        };
    }

    /// <summary>
    /// Returns the bowler's legal name in the format: FirstName [MiddleName] LastName[, Suffix].
    /// Use case: Official documents, 1099 tax reporting, legal records.
    /// </summary>
    /// <returns>The legal name string (e.g., "David Michael Smith, Jr.").</returns>
    public string ToLegalName()
    {
        StringBuilder parts = new(FirstName);

        if (!string.IsNullOrWhiteSpace(MiddleName))
        {
            parts.Append(CultureInfo.CurrentCulture, $" {MiddleName}");
        }

        parts.Append(CultureInfo.CurrentCulture, $" {LastName}");

        if (!string.IsNullOrWhiteSpace(Suffix))
        {
            parts.Append(CultureInfo.CurrentCulture, $", {Suffix}");
        }

        return parts.ToString();
    }

    /// <summary>
    /// Returns the bowler's display name: [Nickname|FirstName] LastName.
    /// Uses nickname if available, otherwise uses first name.
    /// Use case: Public website display, tournament results, awards lists.
    /// </summary>
    /// <returns>The display name string (e.g., "Dave Smith" if a nickname exists, otherwise "David Smith").</returns>
    public string ToDisplayName()
        => !string.IsNullOrWhiteSpace(Nickname)
            ? $"{Nickname} {LastName}"
            : $"{FirstName} {LastName}";

    /// <summary>
    /// Returns the bowler's formal name in the format: FirstName LastName (ignoring nickname).
    /// Use case: Formal communications where nicknames are inappropriate.
    /// </summary>
    /// <returns>The formal name string (e.g., "David Smith").</returns>
    public string ToFormalName()
        => $"{FirstName} {LastName}";

    ///<inheritdoc />
    public override string ToString()
        => ToLegalName();
}
