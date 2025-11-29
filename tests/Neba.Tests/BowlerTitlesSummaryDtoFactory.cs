using Bogus;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Bowlers;

namespace Neba.Tests;

public static class BowlerTitlesSummaryDtoFactory
{
    public const string BowlerName = "John Doe";

    public static BowlerTitlesSummaryDto Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null)
    {
        return new BowlerTitlesSummaryDto
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? BowlerName,
            TitleCount = titleCount ?? 5
        };
    }

    public static BowlerTitlesSummaryDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitlesSummaryDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitlesSummaryDto> faker = new Bogus.Faker<BowlerTitlesSummaryDto>()
            .RuleFor(b => b.BowlerId, f => BowlerId.New())
            .RuleFor(b => b.BowlerName, f => f.Name.FullName())
            .RuleFor(b => b.TitleCount, f => f.Random.Int(0, 20));

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
