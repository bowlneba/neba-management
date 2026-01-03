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

    public static Bowler Bogus(IReadOnlyCollection<Tournament> seedTournaments, int? seed = null)
        => Bogus(1, seedTournaments, seed).Single();

    public static IReadOnlyCollection<Bowler> Bogus(
        int count,
        IReadOnlyCollection<Tournament> seedTournaments,
        int? seed = null)
    {
        // Create pools of unique IDs to avoid collisions across tests
        // Pools handle Random instantiation and null/value probability internally
        UniqueIdPool websiteIdPool = UniqueIdPool.Create(
            poolSize: 100000,
            baseOffset: 0,
            seed,
            probabilityOfValue: 0.5f);

        UniqueIdPool applicationIdPool = UniqueIdPool.Create(
            poolSize: 100000,
            baseOffset: 100_000_000,
            seed,
            probabilityOfValue: 0.5f);

        Bogus.Faker<Bowler> faker = new Bogus.Faker<Bowler>()
            .CustomInstantiator(f => new Bowler
            {
                Id = BowlerId.New(),
                Name = NameFactory.Bogus(),
                WebsiteId = websiteIdPool.GetNext(),
                ApplicationId = applicationIdPool.GetNext(),
                Titles = TitleFactory.Bogus(f.Random.Int(0, 10), seedTournaments, seed),
                SeasonAwards = SeasonAwardFactory
                    .BogusBowlerOfTheYear(f.Random.Int(0, 5), seed)
                    .Union(SeasonAwardFactory.BogusHighBlockAward(f.Random.Int(0, 5), seed))
                    .Union(SeasonAwardFactory.BogusHighAverageAward(f.Random.Int(0, 5), seed))
                    .ToList().AsReadOnly(),
                HallOfFameInductions = HallOfFameInductionFactory.Bogus(f.Random.Int(0, 2), seed)
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
