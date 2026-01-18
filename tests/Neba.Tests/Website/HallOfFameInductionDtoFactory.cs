using Bogus;
using Neba.Domain;
using Neba.Domain.Awards;
using Neba.Website.Application.Awards.HallOfFame;

namespace Neba.Tests.Website;

public static class HallOfFameInductionDtoFactory
{
    public static HallOfFameInductionDto Create(
        int? year = null,
        Name? bowlerName = null,
        StoredFile? photo = null,
        IReadOnlyCollection<HallOfFameCategory>? categories = null)
            => new()
            {
                Year = year ?? 2024,
                BowlerName = bowlerName ?? NameFactory.Create("John", "Doe"),
                Photo = photo,
                Categories = categories ?? [HallOfFameCategory.SuperiorPerformance]
            };

    public static HallOfFameInductionDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<HallOfFameInductionDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<HallOfFameInductionDto> faker = new Bogus.Faker<HallOfFameInductionDto>()
            .CustomInstantiator(f => new HallOfFameInductionDto
            {
                Year = f.Date.Past(60).Year,
                BowlerName = NameFactory.Bogus(1).Single(),
                Photo = StoredFileFactory.Bogus(1).Single().OrNull(f, 0.6f),
                Categories = f.PickRandom(HallOfFameCategory.List.ToArray(), f.Random.Int(1, 2)).ToList()
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
