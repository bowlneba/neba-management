
using Neba.Domain;
using Neba.Domain.Identifiers;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Represents a document associated with a tournament.
/// </summary>
public sealed class TournamentFile
{
    /// <summary>
    /// Gets the unique identifier for the tournament document.
    /// </summary>
    public required TournamentFileId Id { get; init; }

    /// <summary>
    /// Gets the tournament associated with the document (Navigation property).
    /// </summary>
    internal Tournament Tournament { get; set; } = null!;

    /// <summary>
    /// Gets the type of the tournament file.
    /// </summary>
    public required TournamentFileType FileType { get; init; }

    /// <summary>
    /// Gets the stored file associated with the tournament file.
    /// </summary>
    public required StoredFile File { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the file was uploaded.
    /// </summary>
    public required DateTimeOffset UploadedAtUtc { get; init; }
}
