using Bogus;
using Neba.Domain;
using Neba.Domain.Tournaments;
using Neba.Website.Application.Tournaments;

namespace Neba.Tests.Website;

public static class TitleDtoFactory
{
    public static TitleDto Create(
        DateOnly? tournamentDate = null,
        TournamentType? tournamentType = null
    )
        => new()
        {
            TournamentDate = tournamentDate ?? new DateOnly(2000, 1, 1),
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
            .RuleFor(title => title.TournamentDate, f => DateOnly.FromDateTime(f.Date.Past(70)))
            .RuleFor(title => title.TournamentType, f => f.PickRandom(TournamentType.List.ToArray()));

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
