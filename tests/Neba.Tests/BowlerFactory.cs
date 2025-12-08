using Neba.Domain.Awards;
using Neba.Domain.Bowlers;
using Neba.Domain.Bowlers.BowlerAwards;
using Neba.Domain.Tournaments;

namespace Neba.Tests;

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
        Bogus.Faker<Bowler> faker = new Bogus.Faker<Bowler>()
            .CustomInstantiator(f =>
            {
                var bowlerId = BowlerId.New();
                return new Bowler
                {
                    Id = bowlerId,
                    Name = NameFactory.Bogus(),
                    WebsiteId = f.Random.Bool(0.5f) ? f.Random.Int(1, 1000) : null,
                    ApplicationId = f.Random.Bool(0.5f) ? f.Random.Int(1, 1000) : null,
                    Titles = TitleFactory.Bogus(bowlerId, f.Random.Int(0, 10), seed),
                    SeasonAwards = SeasonAwardFactory.BogusBowlerOfTheYear(bowlerId, f.Random.Int(0, 5), seed)
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
