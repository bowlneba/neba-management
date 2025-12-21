using System.Globalization;
using Bogus;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Awards;

namespace Neba.Tests.Website;

public static class SeasonAwardFactory
{
    public static SeasonAward CreateBowlerOfTheYear(
        string? season = null,
        BowlerOfTheYearCategory? category = null)
    {
        return new SeasonAward
        {
            Id = SeasonAwardId.New(),
            AwardType = SeasonAwardType.BowlerOfTheYear,
            Season = season ?? "2024-2025",
            BowlerOfTheYearCategory = category ?? BowlerOfTheYearCategory.Open,
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
            .RuleFor(boy => boy.BowlerOfTheYearCategory, f => f.PickRandom(BowlerOfTheYearCategory.List.ToArray()));

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
            .RuleFor(award => award.HighBlockScore, f => f.Random.Int(1200, 1400));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }

    public static SeasonAward CreateHighBlockAward(
        string? season = null,
        int? score = null)
        => new()
        {
            Id = SeasonAwardId.New(),
            AwardType = SeasonAwardType.High5GameBlock,
            Season = season ?? "2025",
            HighBlockScore = score ?? 1300
        };

    public static SeasonAward BogusHighAverageAward(int? seed = null)
        => BogusHighAverageAward(BowlerId.New(), 1, seed).Single();

    public static IReadOnlyCollection<SeasonAward> BogusHighAverageAward(
        BowlerId bowlerId,
        int count,
        int? seed = null)
    {
        Faker<SeasonAward> faker = new Faker<SeasonAward>()
            .RuleFor(award => award.Id, _ => SeasonAwardId.New())
            .RuleFor(award => award.AwardType, _ => SeasonAwardType.HighAverage)
            .RuleFor(award => award.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture))
            .RuleFor(award => award.Average, f => f.Random.Int(180, 240))
            .RuleFor(award => award.SeasonTotalGames, f => f.Random.Int(90, 120))
            .RuleFor(award => award.Tournaments, f => f.Random.Int(8, 12));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }

    public static SeasonAward CreateHighGameAward(
        string? season = null,
        int? score = null,
        int? games = null,
        int? tournaments = null)
        => new()
        {
            Id = SeasonAwardId.New(),
            AwardType = SeasonAwardType.HighAverage,
            Season = season ?? "2024-2025",
            Average = score ?? 200,
            SeasonTotalGames = games ?? 99,
            Tournaments = tournaments ?? 9
        };
}
