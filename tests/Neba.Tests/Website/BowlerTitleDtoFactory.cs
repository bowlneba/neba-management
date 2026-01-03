using Bogus;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.Tests.Website;

public static class BowlerTitleDtoFactory
{
    public const string BowlerName = "John Doe";

    public static BowlerTitleDto Create(
        BowlerId? bowlerId = null,
        Name? bowlerName = null,
        DateOnly? tournamentDate = null,
        TournamentType? tournamentType = null
        )
            => new()
            {
                BowlerId = bowlerId ?? BowlerId.New(),
                BowlerName = bowlerName ?? NameFactory.Create(),
                TournamentDate = tournamentDate ?? new DateOnly(2000, 1, 1),
                TournamentType = tournamentType ?? TournamentType.Singles
            };

    public static BowlerTitleDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleDto> Bogus(
        int count,
        int? seed = null
        )
    {
        Faker<BowlerTitleDto> faker = new Faker<BowlerTitleDto>()
            .RuleFor(bowler => bowler.BowlerId, _ => BowlerId.New())
            .RuleFor(bowler => bowler.BowlerName, _ => NameFactory.Bogus(1).Single())
            .RuleFor(bowler => bowler.TournamentDate, f => DateOnly.FromDateTime(f.Date.Past(70)))
            .RuleFor(bowler => bowler.TournamentType, f => f.PickRandom(TournamentType.List.ToArray()));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
