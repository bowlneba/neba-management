using System.Globalization;
using Bogus;
using Neba.Website.Contracts.Awards;

namespace Neba.Tests.Website;

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
            .CustomInstantiator(f => new BowlerOfTheYearResponse
            {
                BowlerName = f.Name.FullName(),
                Season = f.Date.Past(60).Year.ToString(CultureInfo.InvariantCulture),
                Category = f.PickRandom("Open", "Woman", "Senior", "Super Senior", "Youth", "Rookie")
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
