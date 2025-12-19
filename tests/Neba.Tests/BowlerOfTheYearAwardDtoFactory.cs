using System.Globalization;
using Bogus;
using Neba.Application.Awards.BowlerOfTheYear;
using Neba.Domain.Awards;
using Neba.Domain.Identifiers;

namespace Neba.Tests;

public static class BowlerOfTheYearAwardDtoFactory
{
    public const string BowlerName = "Jane Smith";

    public static BowlerOfTheYearAwardDto Create(
        string? bowlerName = null,
        string? season = null,
        BowlerOfTheYearCategory? category = null,
        BowlerId? bowlerId = null)
    {
        return new BowlerOfTheYearAwardDto
        {
            Id = SeasonAwardId.New(),
            BowlerName = bowlerName ?? BowlerName,
            Season = season ?? "2024-2025",
            Category = category ?? BowlerOfTheYearCategory.Open,
            BowlerId = bowlerId ?? BowlerId.New()
        };
    }

    public static BowlerOfTheYearAwardDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerOfTheYearAwardDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerOfTheYearAwardDto> faker = new Faker<BowlerOfTheYearAwardDto>()
            .RuleFor(boy => boy.Id, _ => SeasonAwardId.New())
            .RuleFor(boy => boy.BowlerName, f => f.Person.FullName)
            .RuleFor(boy => boy.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture))
            .RuleFor(boy => boy.Category, f => f.PickRandom(BowlerOfTheYearCategory.List.ToArray()))
            .RuleFor(boy => boy.BowlerId, _ => BowlerId.New());

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
