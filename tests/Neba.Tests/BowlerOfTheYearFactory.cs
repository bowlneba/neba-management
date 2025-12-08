using System.Globalization;
using Bogus;
using Neba.Domain.Awards;
using Neba.Domain.Bowlers;
using Neba.Domain.Bowlers.BowlerAwards;

namespace Neba.Tests;

public static class BowlerOfTheYearFactory
{
    public static BowlerOfTheYear Create(
        BowlerOfTheYearId? id = null,
        string? season = null,
        BowlerOfTheYearCategory? category = null,
        BowlerId? bowlerId = null)
    {
        return new BowlerOfTheYear
        {
            Id = id ?? BowlerOfTheYearId.New(),
            Season = season ?? "2024-2025",
            Category = category ?? BowlerOfTheYearCategory.Open,
            BowlerId = bowlerId ?? BowlerId.New()
        };
    }

    public static BowlerOfTheYear Bogus(BowlerId bowlerId, int? seed = null)
        => Bogus(bowlerId, 1, seed).Single();

    public static IReadOnlyCollection<BowlerOfTheYear> Bogus(
        BowlerId bowlerId,
        int count,
        int? seed = null)
    {
        Faker<BowlerOfTheYear> faker = new Faker<BowlerOfTheYear>()
            .RuleFor(boy => boy.Id, _ => BowlerOfTheYearId.New())
            .RuleFor(boy => boy.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture))
            .RuleFor(boy => boy.Category, f => f.PickRandom(BowlerOfTheYearCategory.List.ToArray()))
            .RuleFor(boy => boy.BowlerId, _ => bowlerId);

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
