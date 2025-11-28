using Bogus;
using Neba.Contracts.History.Champions;

namespace Neba.Tests;

public static class GetBowlerTitleCountsResponseFactory
{
    public static GetBowlerTitleCountsResponse Create(
        Guid? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null)
        => new()
        {
            BowlerId = bowlerId ?? Guid.NewGuid(),
            BowlerName = bowlerName ?? "Jane Smith",
            TitleCount = titleCount ?? 5
        };

    public static GetBowlerTitleCountsResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<GetBowlerTitleCountsResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<GetBowlerTitleCountsResponse> faker = new Faker<GetBowlerTitleCountsResponse>()
            .RuleFor(response => response.BowlerId, f => f.Random.Guid())
            .RuleFor(response => response.BowlerName, f => f.Name.FullName())
            .RuleFor(response => response.TitleCount, f => f.Random.Int(1, 10));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
