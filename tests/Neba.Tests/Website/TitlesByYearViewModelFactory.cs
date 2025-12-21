using Neba.Web.Server.History.Champions;

namespace Neba.Tests.Website;

public static class TitlesByYearViewModelFactory
{
    public static TitlesByYearViewModel Create(
        int? year = null,
        IReadOnlyCollection<BowlerTitleViewModel>? titles = null)
    {
        return new TitlesByYearViewModel
        {
            Year = year ?? 2023,
            Titles = titles ?? new List<BowlerTitleViewModel> { BowlerTitleViewModelFactory.Create() }
        };
    }

    public static TitlesByYearViewModel Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<TitlesByYearViewModel> Bogus(
        int count,
        int? seed = null)
    {
        Bogus.Faker<TitlesByYearViewModel> faker = new Bogus.Faker<TitlesByYearViewModel>()
            .CustomInstantiator(f =>
            {
                return new TitlesByYearViewModel
                {
                    Year = f.Random.Int(2000, 2025),
                    Titles = BowlerTitleViewModelFactory.Bogus(f.Random.Int(1, 5), seed)
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
