using Neba.Domain;

namespace Neba.Website.Application.Tournaments;

/// <summary>
/// Provides URL building capabilities for tournament-related resources.
/// </summary>
/// <remarks>
/// This abstraction allows swapping storage providers (Azure → AWS S3)
/// without changing application or domain code.
/// </remarks>
public interface ITournamentUrlBuilder
{
    /// <summary>
    /// Builds a publicly accessible URL for a stored file.
    /// </summary>
    /// <param name="file">The stored file metadata.</param>
    /// <returns>A <see cref="Uri"/> pointing to the file in blob storage, or <c>null</c> if file is <c>null</c>.</returns>
    Uri? BuildFileUrl(StoredFile? file);
}
