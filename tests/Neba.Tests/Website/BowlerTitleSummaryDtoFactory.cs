using Bogus;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.Tests.Website;

public static class BowlerTitleSummaryDtoFactory
{
    public const string BowlerName = "John Doe";

    public static BowlerTitleSummaryDto Create(
        BowlerId? bowlerId = null,
        Name? bowlerName = null,
        int? titleCount = null,
        bool? hallOfFame = null)
    {
        return new BowlerTitleSummaryDto
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? NameFactory.Create(),
            TitleCount = titleCount ?? 5,
            HallOfFame = hallOfFame ?? false
        };
    }

    public static BowlerTitleSummaryDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleSummaryDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitleSummaryDto> faker = new Bogus.Faker<BowlerTitleSummaryDto>()
            .RuleFor(b => b.BowlerId, _ => BowlerId.New())
            .RuleFor(b => b.BowlerName, _ => NameFactory.Bogus(1).Single())
            .RuleFor(b => b.TitleCount, f => f.Random.Int(0, 20))
            .RuleFor(b => b.HallOfFame, f => f.Random.Bool());

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
