using System.Globalization;
using Bogus;
using Neba.Contracts.Website.Awards;

namespace Neba.Tests;

public static class HighAverageAwardResponseFactory
{
    public const string BowlerName = "Jane Smith";
    public const string Season = "2024-2025";
    public const decimal Average = 215.50m;
    public const int Games = 42;
    public const int Tournaments = 9;

    public static HighAverageAwardResponse Create(
        string? bowlerName = null,
        string? season = null,
        decimal? average = null,
        int? games = null,
        int? tournaments = null)
        => new()
        {
            BowlerName = bowlerName ?? BowlerName,
            Season = season ?? Season,
            Average = average ?? Average,
            Games = games ?? Games,
            Tournaments = tournaments ?? Tournaments
        };

    public static HighAverageAwardResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<HighAverageAwardResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<HighAverageAwardResponse> faker = new Faker<HighAverageAwardResponse>()
            .RuleFor(b => b.BowlerName, f => f.Name.FullName())
            .RuleFor(b => b.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.InvariantCulture))
            .RuleFor(b => b.Average, f => f.Random.Decimal(150, 250))
            .RuleFor(b => b.Games, f => f.Random.Int(30, 100))
            .RuleFor(b => b.Tournaments, f => f.Random.Int(5, 15));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
