using Bogus;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class TournamentFileFactory
{
    public static TournamentFile Create(
        TournamentFileType? fileType = null,
        StoredFile? file = null,
        DateTimeOffset? uploadedAtUtc = null)
            => new()
            {
                Id = TournamentFileId.New(),
                FileType = fileType ?? TournamentFileType.Logo,
                File = file ?? StoredFileFactory.Create(
                    container: "tournament-files",
                    path: "logos/tournament-logo.png",
                    contentType: "image/png",
                    sizeInBytes: 52480),
                UploadedAtUtc = uploadedAtUtc ?? DateTimeOffset.UtcNow
            };

    public static TournamentFile Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<TournamentFile> Bogus(
        int count,
        int? seed = null)
    {
        if (count == 0)
        {
            return [];
        }

        Bogus.Faker<TournamentFile> faker = new Bogus.Faker<TournamentFile>()
            .CustomInstantiator(f =>
            {
                return new TournamentFile
                {
                    Id = TournamentFileId.New(),
                    FileType = TournamentFileType.Logo, // Only type currently available
                    File = new StoredFile
                    {
                        Container = "tournament-files",
                        Path = f.System.FilePath().Replace("\\", "/"),
                        ContentType = f.PickRandom("image/png", "image/jpeg", "image/gif"),
                        SizeInBytes = f.Random.Long(10_000, 5_000_000)
                    },
                    UploadedAtUtc = f.Date.RecentOffset(30)
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
