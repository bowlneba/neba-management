using Bogus;
using Neba.Domain.Identifiers;
using Neba.Website.Contracts.Bowlers;

namespace Neba.Tests.Website;

public static class BowlerTitlesResponseFactory
{
    public static BowlerTitlesResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitlesResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitlesResponse> faker = new Faker<BowlerTitlesResponse>()
            .RuleFor(response => response.BowlerId, _ => BowlerId.New())
            .RuleFor(response => response.BowlerName, f => f.Name.FullName())
            .RuleFor(response => response.Titles, f => BowlerTitleResponseFactory.Bogus(f.Random.Int(1, 10), seed));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }

    public static BowlerTitlesResponse Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null)
        => new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? "John Doe",
            Titles = BowlerTitleResponseFactory.Bogus(titleCount ?? 5)
        };

    public static BowlerTitlesResponse Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        IReadOnlyCollection<BowlerTitleResponse>? titles = null)
    {
        return new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? "John Doe",
            Titles = titles ?? BowlerTitleResponseFactory.Bogus(5)
        };
    }
}
