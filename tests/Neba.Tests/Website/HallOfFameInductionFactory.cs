using Bogus;
using Neba.Domain;
using Neba.Domain.Awards;
using Neba.Website.Domain.Awards;

namespace Neba.Tests.Website;

public static class HallOfFameInductionFactory
{
    public static HallOfFameInduction Create(
        int? year = null,
        StoredFile? photo = null,
        IReadOnlyCollection<HallOfFameCategory>? categories = null)
            => new()
            {
                Year = year ?? 2024,
                Photo = photo,
                Categories = categories ?? [ HallOfFameCategory.SuperiorPerformance ]
            };

    public static HallOfFameInduction Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<HallOfFameInduction> Bogus(
        int count,
        int? seed = null)
    {
        Bogus.Faker<HallOfFameInduction> faker = new Bogus.Faker<HallOfFameInduction>()
            .RuleFor(hof => hof.Year, f => f.Date.Past(60).Year)
            .RuleFor(hof => hof.Photo, f => StoredFileFactory.Bogus(1).Single().OrNull(f, 0.6f))
            .RuleFor(hof => hof.Categories, f => f.PickRandom(HallOfFameCategory.List.ToArray(), f.Random.Int(1,2)).ToList());

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
