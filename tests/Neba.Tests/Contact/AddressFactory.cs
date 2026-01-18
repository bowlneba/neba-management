using Bogus;
using ErrorOr;
using Neba.Domain.Contact;
using Neba.Domain.Geography;

namespace Neba.Tests.Contact;

public static class AddressFactory
{
    public const string ValidStreet = "123 Main St";
    public const string ValidCity = "Springfield";
    public const string ValidRegion = "IL";
    public static readonly Country ValidCountry = Country.UnitedStates;
    public const string ValidPostalCode = "62704";
    public const double DefaultLatitude = 39.7817;  // Springfield, IL
    public const double DefaultLongitude = -89.6501;

    public static Address Create(
        string? street = null,
        string? unit = null,
        string? city = null,
        string? region = null,
        Country? country = null,
        string? postalCode = null,
        Coordinates? coordinates = null)
    {
        Coordinates defaultCoordinates = coordinates ?? Coordinates.Create(DefaultLatitude, DefaultLongitude).Value;

        return new()
        {
            Street = street ?? ValidStreet,
            Unit = unit,
            City = city ?? ValidCity,
            Region = region ?? ValidRegion,
            Country = country ?? ValidCountry,
            PostalCode = postalCode ?? ValidPostalCode,
            Coordinates = defaultCoordinates
        };
    }

    public static Address Bogus(int? seed = null)
    {
        Faker<Address> faker = new Bogus.Faker<Address>()
            .CustomInstantiator(faker =>
            {
                double lat = faker.Address.Latitude();
                double lon = faker.Address.Longitude();
                ErrorOr<Coordinates> coordResult = Coordinates.Create(lat, lon);
                Coordinates? coordinates = coordResult.IsError ? null : coordResult.Value;

                return new Address()
                {
                    Street = faker.Address.StreetAddress(),
                    Unit = faker.Random.Bool(0.3f) ? faker.Address.SecondaryAddress() : null,
                    City = faker.Address.City(),
                    Region = faker.Address.StateAbbr(),
                    Country = Country.UnitedStates,
                    PostalCode = faker.Address.ZipCode(),
                    Coordinates = coordinates
                };
            });

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate();
    }
}
