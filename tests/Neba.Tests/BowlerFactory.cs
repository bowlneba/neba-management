using Neba.Domain.Bowlers;

namespace Neba.Tests;

public static class BowlerFactory
{
    public static Bowler Create(
        BowlerId? id = null,
        Name? name = null,
        int? websiteId = null,
        int? applicationId = null,
        IReadOnlyCollection<Domain.Tournaments.Title>? titles = null)
            => new()
            {
                Id = id ?? BowlerId.New(),
                Name = name ?? NameFactory.Create(),
                WebsiteId = websiteId,
                ApplicationId = applicationId,
                Titles = titles ?? []
            };

    public static Bowler Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<Bowler> Bogus(
        int count,
        int? seed = null)
    {
        Bogus.Faker<Bowler> faker = new Bogus.Faker<Bowler>()
            .RuleFor(bowler => bowler.Id, _ => BowlerId.New())
            .RuleFor(bowler => bowler.Name, _ => NameFactory.Bogus())
            .RuleFor(bowler => bowler.WebsiteId, f => f.Random.Bool(0.5f) ? f.Random.Int(1, 1000) : null)
            .RuleFor(bowler => bowler.ApplicationId, f => f.Random.Bool(0.5f) ? f.Random.Int(1, 1000) : null)
            .RuleFor(bowler => bowler.Titles, (f, b) => TitleFactory.Bogus(b.Id, f.Random.Int(0, 10), seed));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
