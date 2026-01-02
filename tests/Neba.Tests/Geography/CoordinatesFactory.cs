using Neba.Domain.Geography;

namespace Neba.Tests.Geography;

public static class CoordinatesFactory
{
    public static Coordinates Create(
        double? latitude = null,
        double? longitude = null)
        => new()
        {
            Latitude = latitude ?? 45.4215,
            Longitude = longitude ?? -75.6972
        };

    public static Coordinates Bogus(int? seed = null)
    {
        var faker = new Bogus.Faker();

        if (seed.HasValue)
        {
            faker.Random = new Bogus.Randomizer(seed.Value);
        }

        return new Coordinates
        {
            Latitude = faker.Address.Latitude(),
            Longitude = faker.Address.Longitude()
        };
    }
}
