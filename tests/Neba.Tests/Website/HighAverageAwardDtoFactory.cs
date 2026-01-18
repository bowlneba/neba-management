using System.Globalization;
using Bogus;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Application.Awards.HighAverage;

namespace Neba.Tests.Website;

public static class HighAverageAwardDtoFactory
{
    public const string BowlerName = "John Doe";

    public static HighAverageAwardDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<HighAverageAwardDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<HighAverageAwardDto> faker = new Faker<HighAverageAwardDto>()
            .CustomInstantiator(f => new HighAverageAwardDto
            {
                Id = SeasonAwardId.New(),
                BowlerName = NameFactory.Bogus(1).Single(),
                Average = f.Random.Decimal(190.0m, 220.0m),
                Games = f.Random.Int(8, 12),
                Tournaments = f.Random.Int(3, 7),
                Season = f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture)
            });

        if (seed is not null)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }

    public static HighAverageAwardDto Create(
        string? season = null,
        Name? bowlerName = null,
        decimal? average = null,
        int? gamesBowled = null,
        int? tournaments = null)
            => new()
            {
                Id = SeasonAwardId.New(),
                Season = season ?? "2023",
                BowlerName = bowlerName ?? NameFactory.Create(),
                Average = average ?? 210.5m,
                Games = gamesBowled ?? 10,
                Tournaments = tournaments ?? 5
            };
}
