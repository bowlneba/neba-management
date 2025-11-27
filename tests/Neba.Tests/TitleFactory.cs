using Neba.Domain;
using Neba.Domain.Bowlers;
using Neba.Domain.Tournaments;

namespace Neba.Tests;

public static class TitleFactory
{
    public static Title Create(
        TitleId? id = null,
        BowlerId? bowlerId = null,
        TournamentType? tournamentType = null,
        Month? month = null,
        int? year = null)
            => new()
            {
                Id = id ?? TitleId.New(),
                BowlerId = bowlerId ?? BowlerId.New(),
                TournamentType = tournamentType ?? TournamentType.Singles,
                Month = month ?? Month.January,
                Year = year ?? 2000
            };

    public static IReadOnlyCollection<Title> Bogus(
        BowlerId bowlerId,
        int titleCount,
        int? seed = null)
    {
        Bogus.Faker<Title> faker = new Bogus.Faker<Title>()
            .RuleFor(title => title.Id, _ => TitleId.New())
            .RuleFor(title => title.BowlerId, _ => bowlerId)
            .RuleFor(title => title.TournamentType, f => f.PickRandom(TournamentType.List.ToArray()))
            .RuleFor(title => title.Month, f => f.PickRandom(Month.List.ToArray()))
            .RuleFor(title => title.Year, f => f.Date.Between(new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Local), DateTime.UtcNow).Year);

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(titleCount);
    }
}
