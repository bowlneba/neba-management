using System.Globalization;
using Bogus;
using Neba.Website.Contracts.Awards;

namespace Neba.Tests.Website;

public static class HighBlockAwardResponseFactory
{
    public const string BowlerName = "John Smith";
    public const string Season = "2024-2025";
    public const int Score = 1150;

    public static HighBlockAwardResponse Create(
        string? bowlerName = null,
        string? season = null,
        int? score = null)
        => new()
        {
            BowlerName = bowlerName ?? BowlerName,
            Season = season ?? Season,
            Score = score ?? Score
        };

    public static HighBlockAwardResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<HighBlockAwardResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<HighBlockAwardResponse> faker = new Faker<HighBlockAwardResponse>()
            .RuleFor(b => b.BowlerName, f => f.Name.FullName())
            .RuleFor(b => b.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.InvariantCulture))
            .RuleFor(b => b.Score, f => f.Random.Int(900, 1300));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
