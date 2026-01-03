using Neba.Domain.Identifiers;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class TitleFactory
{
    public static Title Create(
        TitleId? id = null,
        BowlerId? bowlerId = null,
        Tournament? tournament = null)
    {
        tournament ??= TournamentFactory.Create();
        return new()
        {
            Id = id ?? TitleId.New(),
            BowlerId = bowlerId ?? BowlerId.New(),
            Tournament = tournament,
            TournamentId = tournament.Id
        };
    }

    public static IReadOnlyCollection<Title> Bogus(
        int titleCount,
        IEnumerable<Tournament> seedTournaments,
        IEnumerable<Bowler> seedBowlers,
        int? seed = null)
    {
        Bogus.Faker<Title> faker = new Bogus.Faker<Title>()
            .CustomInstantiator(f =>
            {
                Tournament tournament = f.PickRandom(seedTournaments);
                Bowler bowler = f.PickRandom(seedBowlers);

                return new Title
                {
                    Id = TitleId.New(),
                    BowlerId = bowler.Id,
                    Tournament = tournament,
                    TournamentId = tournament.Id
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(titleCount);
    }
}
