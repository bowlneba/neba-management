using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class TitleFactory
{
    public static Title Create(
        TitleId? id = null,
        TournamentType? tournamentType = null,
        Month? month = null,
        int? year = null)
            => new()
            {
                Id = id ?? TitleId.New(),
                TournamentType = tournamentType ?? TournamentType.Singles,
                Month = month ?? Month.January,
                Year = year ?? 2000
            };

    public static IReadOnlyCollection<Title> Bogus(
        int titleCount,
        int? seed = null)
    {
        Bogus.Faker<Title> faker = new Bogus.Faker<Title>()
            .CustomInstantiator(f => new Title
            {
                Id = TitleId.New(),
                TournamentType = f.PickRandom(TournamentType.List.ToArray()),
                Month = f.PickRandom(Month.List.ToArray()),
                Year = f.Date.Between(new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Local), DateTime.UtcNow).Year
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(titleCount);
    }
}
