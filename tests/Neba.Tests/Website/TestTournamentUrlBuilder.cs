using Neba.Domain;
using Neba.Website.Application.Tournaments;

namespace Neba.Tests.Website;

/// <summary>
/// Test implementation of ITournamentUrlBuilder for integration tests.
/// </summary>
public sealed class TestTournamentUrlBuilder : ITournamentUrlBuilder
{
    public Uri? BuildFileUrl(StoredFile? file)
    {
        if (file is null)
        {
            return null;
        }

        // Return a predictable test URL that includes the container and path
        return new Uri($"https://test-storage.local/{file.Container}/{file.Path}");
    }
}
