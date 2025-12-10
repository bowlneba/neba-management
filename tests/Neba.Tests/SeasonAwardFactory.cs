using System.Globalization;
using Bogus;
using Neba.Domain.Awards;
using Neba.Domain.Bowlers;

namespace Neba.Tests;

public static class SeasonAwardFactory
{
    public static SeasonAward CreateBowlerOfTheYear(
        string? season = null,
        BowlerOfTheYearCategory? category = null,
        BowlerId? bowlerId = null)
    {
        return new SeasonAward
        {
            Id = SeasonAwardId.New(),
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

    public static SeasonAward BogusHighBlockAward(int? seed = null)
        => BogusHighBlockAward(BowlerId.New(), 1, seed).Single();

    public static IReadOnlyCollection<SeasonAward> BogusHighBlockAward(
        BowlerId bowlerId,
        int count,
        int? seed = null)
    {
        Faker<SeasonAward> faker = new Faker<SeasonAward>()
            .RuleFor(award => award.Id, _ => SeasonAwardId.New())
            .RuleFor(award => award.AwardType, _ => SeasonAwardType.High5GameBlock)
            .RuleFor(award => award.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture))
            .RuleFor(award => award.BowlerId, _ => bowlerId)
            .RuleFor(award => award.HighBlockScore, f => f.Random.Int(1200, 1400));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }

    public static SeasonAward CreateHighBlockAward(
        string? season = null,
        BowlerId? bowlerId = null,
        int? score = null)
        => new SeasonAward
        {
            Id = SeasonAwardId.New(),
            AwardType = SeasonAwardType.High5GameBlock,
            Season = season ?? "2025",
            BowlerId = bowlerId ?? BowlerId.New(),
            HighBlockScore = score ?? 1300
        };
}
