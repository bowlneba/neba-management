using System.Globalization;
using Bogus;
using Neba.Domain.Awards;
using Neba.Domain.Bowlers;

namespace Neba.Tests;

public static class SeasonAwardFactory
{
    public static SeasonAward CreateBowlerOfTheYear(
        SeasonAwardId? id = null,
        string? season = null,
        BowlerOfTheYearCategory? category = null,
        BowlerId? bowlerId = null)
    {
        return new SeasonAward
        {
            Id = id ?? SeasonAwardId.New(),
            AwardType = SeasonAwardType.BowlerOfTheYear,
            Season = season ?? "2024-2025",
            BowlerOfTheYearCategory = category ?? BowlerOfTheYearCategory.Open,
            BowlerId = bowlerId ?? BowlerId.New()
        };
    }

    public static SeasonAward BogusBowlerOfTheYear(BowlerId bowlerId, int? seed = null)
        => BogusBowlerOfTheYear(bowlerId, 1, seed).Single();

    public static IReadOnlyCollection<SeasonAward> BogusBowlerOfTheYear(
        BowlerId bowlerId,
        int count,
        int? seed = null)
    {
        Faker<SeasonAward> faker = new Faker<SeasonAward>()
            .RuleFor(boy => boy.Id, _ => SeasonAwardId.New())
            .RuleFor(boy => boy.AwardType, _ => SeasonAwardType.BowlerOfTheYear)
            .RuleFor(boy => boy.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture))
            .RuleFor(boy => boy.BowlerOfTheYearCategory, f => f.PickRandom(BowlerOfTheYearCategory.List.ToArray()))
            .RuleFor(boy => boy.BowlerId, _ => bowlerId);

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
