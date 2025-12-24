using System.Globalization;
using Bogus;
using Neba.Domain.Identifiers;
using Neba.Website.Application.Awards.HighBlock;

namespace Neba.Tests.Website;

public static class HighBlockAwardDtoFactory
{
    public const string BowlerName = "John Doe";

    public static HighBlockAwardDto Create(
        string? bowlerName = null,
        string? season = null,
        int? score = null
    )
        => new()
        {
            Id = SeasonAwardId.New(),
            BowlerName = bowlerName ?? BowlerName,
            Season = season ?? "2023",
            Score = score ?? 1300
        };

    public static HighBlockAwardDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<HighBlockAwardDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<HighBlockAwardDto>? faker = new Faker<HighBlockAwardDto>()
            .RuleFor(award => award.Id, _ => SeasonAwardId.New())
            .RuleFor(award => award.BowlerName, f => f.Name.FullName())
            .RuleFor(award => award.Season, f => f.Date.Past(60).Year.ToString(CultureInfo.CurrentCulture))
            .RuleFor(award => award.Score, f => f.Random.Int(1200, 1350));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
