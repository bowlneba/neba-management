using Ardalis.SmartEnum;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Represents the type of a tournament file.
/// </summary>
public sealed class TournamentFileType
    : SmartEnum<TournamentFileType>
{
    /// <summary>
    /// The logo file type.
    /// </summary>
    public static readonly TournamentFileType Logo = new(nameof(Logo), 1);
    private TournamentFileType(string name, int value)
        : base(name, value)
    { }
}
