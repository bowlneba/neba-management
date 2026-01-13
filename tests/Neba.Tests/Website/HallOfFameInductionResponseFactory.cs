using Bogus;
using Neba.Website.Contracts.Awards;

namespace Neba.Tests.Website;

public static class HallOfFameInductionResponseFactory
{
    public const string BowlerName = "John Smith";
    public const int Year = 2020;

    public static HallOfFameInductionResponse Create(
        string? bowlerName = null,
        int? year = null,
        Uri? photoUrl = null,
        IReadOnlyCollection<string>? categories = null)
        => new()
        {
            BowlerName = bowlerName ?? BowlerName,
            Year = year ?? Year,
            PhotoUrl = photoUrl,
            Categories = categories ?? ["Superior Performance"]
        };

    public static HallOfFameInductionResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<HallOfFameInductionResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<HallOfFameInductionResponse> faker = new Faker<HallOfFameInductionResponse>()
            .CustomInstantiator(f =>
            {
                string[] availableCategories = ["Superior Performance", "Meritorious Service", "Friend of NEBA"];
                int categoryCount = f.Random.Int(1, 2);

                return new HallOfFameInductionResponse
                {
                    BowlerName = f.Name.FullName(),
                    Year = f.Date.Past(60).Year,
                    PhotoUrl = f.Random.Bool(0.4f) ? new Uri(f.Internet.Avatar()) : null,
                    Categories = f.PickRandom(availableCategories, categoryCount).ToList()
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
