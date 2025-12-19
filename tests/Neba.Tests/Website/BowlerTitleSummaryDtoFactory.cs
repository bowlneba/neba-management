using Bogus;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Identifiers;

namespace Neba.Tests.Website;

public static class BowlerTitleSummaryDtoFactory
{
    public const string BowlerName = "John Doe";

    public static BowlerTitleSummaryDto Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null)
    {
        return new BowlerTitleSummaryDto
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? BowlerName,
            TitleCount = titleCount ?? 5
        };
    }

    public static BowlerTitleSummaryDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleSummaryDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitleSummaryDto> faker = new Bogus.Faker<BowlerTitleSummaryDto>()
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
