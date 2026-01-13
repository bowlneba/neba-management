using Bogus;
using Neba.Domain.Identifiers;
using Neba.Web.Server.History.Champions;

namespace Neba.Tests.Website;

public static class BowlerTitleSummaryViewModelFactory
{
    public const string DefaultBowlerName = "John Doe";
    public const int DefaultTitleCount = 5;

    public static BowlerTitleSummaryViewModel Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null,
        bool? hallOfFame = null)
        => new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? DefaultBowlerName,
            TitleCount = titleCount ?? DefaultTitleCount,
            HallOfFame = hallOfFame ?? false
        };

    public static BowlerTitleSummaryViewModel Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleSummaryViewModel> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitleSummaryViewModel> faker = new Faker<BowlerTitleSummaryViewModel>()
            .CustomInstantiator(f => new BowlerTitleSummaryViewModel
            {
                BowlerId = BowlerId.New(),
                BowlerName = f.Name.FullName(),
                TitleCount = f.Random.Int(1, 30),
                HallOfFame = f.Random.Bool(0.2f)
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
