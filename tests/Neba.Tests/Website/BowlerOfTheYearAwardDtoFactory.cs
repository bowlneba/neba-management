using System.Globalization;
using Bogus;
using Neba.Domain;
using Neba.Domain.Awards;
using Neba.Domain.Identifiers;
using Neba.Website.Application.Awards.BowlerOfTheYear;

namespace Neba.Tests.Website;

public static class BowlerOfTheYearAwardDtoFactory
{
    public const string BowlerName = "Jane Smith";

    public static BowlerOfTheYearAwardDto Create(
        Name? bowlerName = null,
        string? season = null,
        BowlerOfTheYearCategory? category = null,
        BowlerId? bowlerId = null)
    {
        return new BowlerOfTheYearAwardDto
        {
            Id = SeasonAwardId.New(),
            BowlerName = bowlerName ?? NameFactory.Create(),
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
            .CustomInstantiator(f => new BowlerOfTheYearAwardDto
            {
                Id = SeasonAwardId.New(),
                BowlerName = NameFactory.Bogus(1).Single(),
                Season = f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture),
                Category = f.PickRandom(BowlerOfTheYearCategory.List.ToArray()),
                BowlerId = BowlerId.New()
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
