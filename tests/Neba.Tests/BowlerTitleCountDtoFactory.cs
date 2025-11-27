using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Domain.Bowlers;

namespace Neba.Tests;

public static class BowlerTitleCountDtoFactory
{
    public const string BowlerName = "John Public";
    public const int TitleCount = 5;

    public static BowlerTitleCountDto Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null)
        => new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? BowlerName,
            TitleCount = titleCount ?? TitleCount
        };

    public static BowlerTitleCountDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleCountDto> Bogus(
        int count,
        int? seed = null)
    {
        Bogus.Faker<BowlerTitleCountDto> faker = new Bogus.Faker<BowlerTitleCountDto>()
            .RuleFor(dto => dto.BowlerId, _ => BowlerId.New())
            .RuleFor(dto => dto.BowlerName, f => f.Name.FullName())
            .RuleFor(dto => dto.TitleCount, f => f.Random.Int(0, 20));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
