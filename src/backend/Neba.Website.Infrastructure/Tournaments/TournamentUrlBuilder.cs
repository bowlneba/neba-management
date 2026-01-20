using Neba.Application.Storage;
using Neba.Domain;
using Neba.Website.Application.Tournaments;

namespace Neba.Website.Infrastructure.Tournaments;

/// <summary>
/// Builds URLs for tournament-related resources using the configured storage service.
/// </summary>
internal sealed class TournamentUrlBuilder(IStorageService storageService) : ITournamentUrlBuilder
{
    public Uri? BuildFileUrl(StoredFile? file)
    {
        if (file is null)
        {
            return null;
        }

        return storageService.GetBlobUri(file.Container, file.Path);
    }
}
