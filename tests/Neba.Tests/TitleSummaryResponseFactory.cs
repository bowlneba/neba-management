using Bogus;
using Neba.Contracts.Website.Titles;

namespace Neba.Tests;

public static class TitleSummaryResponseFactory
{
    public const string BowlerName = "Joe Bowler";

    public static TitleSummaryResponse Create(
        Guid? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null)
        => new()
        {
            BowlerId = bowlerId ?? Guid.NewGuid(),
            BowlerName = bowlerName ?? BowlerName,
            TitleCount = titleCount ?? 5
        };

    public static TitleSummaryResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<TitleSummaryResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<TitleSummaryResponse> faker = new Faker<TitleSummaryResponse>()
            .RuleFor(b => b.BowlerId, f => f.Random.Guid())
            .RuleFor(b => b.BowlerName, f => f.Name.FullName())
            .RuleFor(b => b.TitleCount, f => f.Random.Int(0, 20));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
