using Bogus;
using Neba.Application.Tournaments;
using Neba.Domain;
using Neba.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class TitleDtoFactory
{
    public static TitleDto Create(
        Month? month = null,
        int? year = null,
        TournamentType? tournamentType = null
    )
        => new()
        {
            Month = month ?? Month.January,
            Year = year ?? 2000,
            TournamentType = tournamentType ?? TournamentType.Singles
        };

    public static TitleDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<TitleDto> Bogus(
        int count,
        int? seed = null
    )
    {
        Faker<TitleDto> faker = new Faker<TitleDto>()
            .RuleFor(title => title.Month, f => f.PickRandom(Month.List.ToArray()))
            .RuleFor(title => title.Year, f => f.Date.Past(70).Year)
            .RuleFor(title => title.TournamentType, f => f.PickRandom(TournamentType.List.ToArray()));

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
