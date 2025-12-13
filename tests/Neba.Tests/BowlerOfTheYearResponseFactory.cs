using System.Globalization;
using Bogus;
using Neba.Contracts.Website.Awards;

namespace Neba.Tests;

public static class BowlerOfTheYearResponseFactory
{
    public const string BowlerName = "Jane Smith";
    public const string Season = "2024-2025";
    public const string Category = "Open";

    public static BowlerOfTheYearResponse Create(
        string? bowlerName = null,
        string? season = null,
        string? category = null)
        => new()
        {
            BowlerName = bowlerName ?? BowlerName,
            Season = season ?? Season,
            Category = category ?? Category
        };

    public static BowlerOfTheYearResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerOfTheYearResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerOfTheYearResponse> faker = new Faker<BowlerOfTheYearResponse>()
            .RuleFor(b => b.BowlerName, f => f.Name.FullName())
            .RuleFor(b => b.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.InvariantCulture))
            .RuleFor(b => b.Category, f => f.PickRandom("Open", "Woman", "Senior", "Super Senior", "Youth", "Rookie"));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
