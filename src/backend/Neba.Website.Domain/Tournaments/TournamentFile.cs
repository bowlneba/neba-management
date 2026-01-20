
using Neba.Domain;
using Neba.Domain.Identifiers;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Represents a file (binary asset) associated with a tournament, stored in blob storage.
/// </summary>
/// <remarks>
/// This represents uploaded files like logos, flyers, or images, NOT documents from Google Docs/Office.
/// See the Documents vs Files section in the ubiquitous language for the distinction.
/// </remarks>
public sealed class TournamentFile
{
    /// <summary>
    /// Gets the unique identifier for the tournament file.
    /// </summary>
    public required TournamentFileId Id { get; init; }

    /// <summary>
    /// Gets the tournament associated with the file (Navigation property).
    /// </summary>
    internal Tournament Tournament { get; set; } = null!;

    /// <summary>
    /// Gets the type of the tournament file (Logo, Flyer, etc.).
    /// </summary>
    public required TournamentFileType FileType { get; init; }

    /// <summary>
    /// Gets the stored file metadata including blob storage location, content type, and size.
    /// </summary>
    public required StoredFile File { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the file was uploaded.
    /// </summary>
    public required DateTimeOffset UploadedAtUtc { get; init; }
}
