using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class TitleFactory
{
    public static Title Create(
        TitleId? id = null,
        BowlerId? bowlerId = null,
        Tournament? tournament = null)
            => new()
            {
                Id = id ?? TitleId.New(),
                BowlerId = bowlerId ?? BowlerId.New(),
                Tournament = tournament ?? TournamentFactory.Create()
            };

    public static IReadOnlyCollection<Title> Bogus(
        int titleCount,
        int? seed = null)
    {
        Bogus.Faker<Title> faker = new Bogus.Faker<Title>()
            .CustomInstantiator(f => new Title
            {
                Id = TitleId.New(),
                BowlerId = BowlerId.New(),
                Tournament = TournamentFactory.Bogus(seed)
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(titleCount);
    }
}
