using Bogus;
using Neba.Domain.Identifiers;
using Neba.Website.Contracts.Titles;

namespace Neba.Tests.Website;

public static class TitleSummaryResponseFactory
{
    public const string BowlerName = "Joe Bowler";

    public static TitleSummaryResponse Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        bool? hallOfFame = null,
        int? titleCount = null)
        => new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? BowlerName,
            TitleCount = titleCount ?? 5,
            HallOfFame = hallOfFame ?? false
        };

    public static TitleSummaryResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<TitleSummaryResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<TitleSummaryResponse> faker = new Faker<TitleSummaryResponse>()
            .CustomInstantiator(f => new TitleSummaryResponse
            {
                BowlerId = BowlerId.New(),
                BowlerName = f.Name.FullName(),
                HallOfFame = f.Random.Bool(),
                TitleCount = f.Random.Int(0, 20)
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
