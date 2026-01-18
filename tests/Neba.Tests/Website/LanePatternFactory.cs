using Neba.Domain.Tournaments;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class LanePatternFactory
{
    public static LanePattern Create(
        PatternLengthCategory? lengthCategory = null,
        PatternRatioCategory? ratioCategory = null)
            => new()
            {
                LengthCategory = lengthCategory ?? PatternLengthCategory.MediumPattern,
                RatioCategory = ratioCategory ?? PatternRatioCategory.Challenge
            };

    public static LanePattern Bogus(int? seed = null)
    {
        Bogus.Faker<LanePattern> faker = new Bogus.Faker<LanePattern>()
            .CustomInstantiator(f => new LanePattern
            {
                LengthCategory = f.PickRandom(PatternLengthCategory.List.ToArray()),
                RatioCategory = f.PickRandom(PatternRatioCategory.List.ToArray())
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate();
    }
}
