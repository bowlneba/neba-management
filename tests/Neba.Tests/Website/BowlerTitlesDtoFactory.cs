using Bogus;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.Tournaments;

namespace Neba.Tests.Website;

public static class BowlerTitlesDtoFactory
{
    public const string BowlerName = "John Doe";

    public static BowlerTitlesDto Create(
        BowlerId? bowlerId = null,
        Name? bowlerName = null,
        bool? hallOfFame = null,
        IReadOnlyCollection<TitleDto>? titles = null
    )
        => new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? NameFactory.Create(),
            HallOfFame = hallOfFame ?? false,
            Titles = titles ?? TitleDtoFactory.Bogus(1)
        };

    public static BowlerTitlesDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitlesDto> Bogus(
        int count,
        int? seed = null
    )
    {
        Faker<BowlerTitlesDto> faker = new Faker<BowlerTitlesDto>()
            .RuleFor(bowler => bowler.BowlerId, _ => BowlerId.New())
            .RuleFor(bowler => bowler.BowlerName, _ => NameFactory.Bogus(1).Single())
            .RuleFor(bowler => bowler.Titles, f => TitleDtoFactory.Bogus(f.Random.Int(0, 5), seed))
            .RuleFor(bowler => bowler.HallOfFame, f => f.Random.Bool());

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
