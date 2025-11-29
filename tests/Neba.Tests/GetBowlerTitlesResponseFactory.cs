using Bogus;
using Neba.Contracts.Website.Bowlers;

namespace Neba.Tests;

public static class GetBowlerTitlesResponseFactory
{
    public static GetBowlerTitlesResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<GetBowlerTitlesResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<GetBowlerTitlesResponse> faker = new Faker<GetBowlerTitlesResponse>()
            .RuleFor(response => response.BowlerId, f => f.Random.Guid())
            .RuleFor(response => response.BowlerName, f => f.Name.FullName())
            .RuleFor(response => response.Titles, f => TitlesResponseFactory.Bogus(f.Random.Int(1, 10), seed));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }

    public static GetBowlerTitlesResponse Create(
        Guid? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null)
        => new()
        {
            BowlerId = bowlerId ?? Guid.NewGuid(),
            BowlerName = bowlerName ?? "John Doe",
            Titles = TitlesResponseFactory.Bogus(titleCount ?? 5)
        };

    public static GetBowlerTitlesResponse Create(
        Guid? bowlerId = null,
        string? bowlerName = null,
        IReadOnlyCollection<TitlesResponse>? titles = null)
    {
        return new()
        {
            BowlerId = bowlerId ?? Guid.NewGuid(),
            BowlerName = bowlerName ?? "John Doe",
            Titles = titles ?? TitlesResponseFactory.Bogus(5)
        };
    }
}
