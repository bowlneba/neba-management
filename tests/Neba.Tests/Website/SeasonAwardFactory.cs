using System.Globalization;
using Bogus;
using Neba.Domain.Awards;
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
            BowlerOfTheYearCategory = category ?? BowlerOfTheYearCategory.Open
        };
    }

    public static SeasonAward BogusBowlerOfTheYear(int? seed = null)
        => BogusBowlerOfTheYear(1, seed).Single();

    public static IReadOnlyCollection<SeasonAward> BogusBowlerOfTheYear(
        int count,
        int? seed = null)
    {
        Faker<SeasonAward> faker = new Faker<SeasonAward>()
            .CustomInstantiator(f => new SeasonAward
            {
                Id = SeasonAwardId.New(),
                AwardType = SeasonAwardType.BowlerOfTheYear,
                Season = f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture),
                BowlerOfTheYearCategory = f.PickRandom(BowlerOfTheYearCategory.List.ToArray())
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }

    public static SeasonAward BogusHighBlockAward(int? seed = null)
        => BogusHighBlockAward(1, seed).Single();

    public static IReadOnlyCollection<SeasonAward> BogusHighBlockAward(
        int count,
        int? seed = null)
    {
        Faker<SeasonAward> faker = new Faker<SeasonAward>()
            .CustomInstantiator(f => new SeasonAward
            {
                Id = SeasonAwardId.New(),
                AwardType = SeasonAwardType.High5GameBlock,
                Season = f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture),
                HighBlockScore = f.Random.Int(1200, 1400)
            });

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
        => BogusHighAverageAward(1, seed).Single();

    public static IReadOnlyCollection<SeasonAward> BogusHighAverageAward(
        int count,
        int? seed = null)
    {
        Faker<SeasonAward> faker = new Faker<SeasonAward>()
            .CustomInstantiator(f => new SeasonAward
            {
                Id = SeasonAwardId.New(),
                AwardType = SeasonAwardType.HighAverage,
                Season = f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture),
                Average = f.Random.Int(180, 240),
                SeasonTotalGames = f.Random.Int(90, 120),
                Tournaments = f.Random.Int(8, 12)
            });

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
