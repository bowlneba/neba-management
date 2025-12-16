using Neba.Domain.Awards;
using Neba.Domain.Bowlers;
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
        var usedWebsiteIds = new HashSet<int>();
        var usedApplicationIds = new HashSet<int>();

        Bogus.Faker<Bowler> faker = new Bogus.Faker<Bowler>()
            .CustomInstantiator(f =>
            {
                var bowlerId = BowlerId.New();

                int? websiteId = null;
                if (f.Random.Bool(0.5f))
                {
                    int candidateId;
                    do
                    {
                        candidateId = f.Random.Int(1, 10000);
                    } while (!usedWebsiteIds.Add(candidateId));
                    websiteId = candidateId;
                }

                int? applicationId = null;
                if (f.Random.Bool(0.5f))
                {
                    int candidateId;
                    do
                    {
                        candidateId = f.Random.Int(1, 10000);
                    } while (!usedApplicationIds.Add(candidateId));
                    applicationId = candidateId;
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
