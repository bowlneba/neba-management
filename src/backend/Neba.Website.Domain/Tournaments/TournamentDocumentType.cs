using Ardalis.SmartEnum;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Represents the type of a tournament document.
/// </summary>
public sealed class TournamentDocumentType
    : SmartEnum<TournamentDocumentType>
{
    /// <summary>
    /// The logo document type.
    /// </summary>
    public static readonly TournamentDocumentType Logo = new(nameof(Logo), 1);
    private TournamentDocumentType(string name, int value)
        : base(name, value)
    { }
}
