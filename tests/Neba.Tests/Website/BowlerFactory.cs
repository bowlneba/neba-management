using Neba.Domain.Awards;
using Neba.Domain.Bowlers;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class BowlerFactory
{
    public static Bowler Create(
        BowlerId? id = null,
        Name? name = null,
        int? websiteId = null,
        int? applicationId = null,
        IReadOnlyCollection<Title>? titles = null,
        IReadOnlyCollection<SeasonAward>? seasonAwards = null)
            => new()
            {
                Id = id ?? BowlerId.New(),
                Name = name ?? NameFactory.Create(),
                WebsiteId = websiteId,
                ApplicationId = applicationId,
                Titles = titles ?? [],
                SeasonAwards = seasonAwards ?? []
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
                var bowlerId = BowlerId.New();

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
                    Id = bowlerId,
                    Name = NameFactory.Bogus(),
                    WebsiteId = websiteId,
                    ApplicationId = applicationId,
                    Titles = TitleFactory.Bogus(bowlerId, f.Random.Int(0, 10), seed),
                    SeasonAwards = SeasonAwardFactory
                        .BogusBowlerOfTheYear(bowlerId, f.Random.Int(0, 5), seed)
                        .Union(SeasonAwardFactory.BogusHighBlockAward(bowlerId, f.Random.Int(0, 5), seed))
                        .Union(SeasonAwardFactory.BogusHighAverageAward(bowlerId, f.Random.Int(0, 5), seed))
                        .ToList().AsReadOnly()
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
