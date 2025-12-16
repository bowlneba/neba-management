using Bogus;
using Neba.Web.Server.History.Champions;

namespace Neba.Tests;

public static class BowlerTitleSummaryViewModelFactory
{
    public const string DefaultBowlerName = "John Doe";
    public const int DefaultTitleCount = 5;

    public static BowlerTitleSummaryViewModel Create(
        Ulid? bowlerId = null,
        string? bowlerName = null,
        int? titleCount = null,
        bool? hallOfFame = null)
        => new()
        {
            BowlerId = bowlerId ?? Ulid.NewUlid(),
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
            .RuleFor(vm => vm.BowlerId, _ => Ulid.NewUlid())
            .RuleFor(vm => vm.BowlerName, f => f.Name.FullName())
            .RuleFor(vm => vm.TitleCount, f => f.Random.Int(1, 30))
            .RuleFor(vm => vm.HallOfFame, f => f.Random.Bool(0.2f));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
