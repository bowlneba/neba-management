using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Awards;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class BowlerFactory
{
    public static Bowler Create(
        BowlerId? id = null,
        Name? name = null,
        int? websiteId = null,
        int? applicationId = null,
        IReadOnlyCollection<Title>? titles = null,
        IReadOnlyCollection<SeasonAward>? seasonAwards = null,
        IReadOnlyCollection<HallOfFameInduction>? hallOfFameInductions = null)
            => new()
            {
                Id = id ?? BowlerId.New(),
                Name = name ?? NameFactory.Create(),
                WebsiteId = websiteId,
                ApplicationId = applicationId,
                Titles = titles ?? [],
                SeasonAwards = seasonAwards ?? [],
                HallOfFameInductions = hallOfFameInductions ?? []
            };

    public static Bowler Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<Bowler> Bogus(
        int count,
        int? seed = null)
    {
#pragma warning disable CA5394 // Random is acceptable here - used only for test data generation, not security
        Random random = seed.HasValue ? new Random(seed.Value) : new Random();

        // Generate unique IDs using larger ranges and timestamp to avoid collisions across tests
        long baseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var shuffledWebsiteIds = Enumerable.Range(1, 100000)
            .Select(i => (int)(baseTimestamp % 1_000_000) + (i * 1000) + random.Next(1000))
            .OrderBy(_ => random.Next())
            .ToList();
        var shuffledApplicationIds = Enumerable.Range(1, 100000)
            .Select(i => (int)(baseTimestamp % 1_000_000) + 100_000_000 + (i * 1000) + random.Next(1000))
            .OrderBy(_ => random.Next())
            .ToList();
#pragma warning restore CA5394
        int websiteIdIndex = 0;
        int applicationIdIndex = 0;

        Bogus.Faker<Bowler> faker = new Bogus.Faker<Bowler>()
            .CustomInstantiator(f =>
            {
                int? websiteId = null;
                if (f.Random.Bool(0.5f))
                {
                    websiteId = shuffledWebsiteIds[websiteIdIndex++];
                }

                int? applicationId = null;
                if (f.Random.Bool(0.5f))
                {
                    applicationId = shuffledApplicationIds[applicationIdIndex++];
                }

                return new Bowler
                {
                    Id = BowlerId.New(),
                    Name = NameFactory.Bogus(),
                    WebsiteId = websiteId,
                    ApplicationId = applicationId,
                    Titles = TitleFactory.Bogus(f.Random.Int(0, 10), seed),
                    SeasonAwards = SeasonAwardFactory
                        .BogusBowlerOfTheYear(f.Random.Int(0, 5), seed)
                        .Union(SeasonAwardFactory.BogusHighBlockAward(f.Random.Int(0, 5), seed))
                        .Union(SeasonAwardFactory.BogusHighAverageAward(f.Random.Int(0, 5), seed))
                        .ToList().AsReadOnly(),
                    HallOfFameInductions = HallOfFameInductionFactory.Bogus(f.Random.Int(0, 2), seed)
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
