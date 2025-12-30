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
    public static Address Create(
        string? street = null,
        string? unit = null,
        string? city = null,
        string? region = null,
        Country? country = null,
        string? postalCode = null,
        Coordinates? coordinates = null)
            => new()
            {
                Street = street ?? ValidStreet,
                Unit = unit,
                City = city ?? ValidCity,
                Region = region ?? ValidRegion,
                Country = country ?? ValidCountry,
                PostalCode = postalCode ?? ValidPostalCode,
                Coordinates = coordinates
            };

    public static Address Bogus(int? seed = null)
    {
        var faker = new Bogus.Faker<Address>()
            .RuleFor(a => a.Street, f => f.Address.StreetAddress())
            .RuleFor(a => a.Unit, f => f.Random.Bool(0.3f) ? f.Address.SecondaryAddress() : null)
            .RuleFor(a => a.City, f => f.Address.City())
            .RuleFor(a => a.Region, f => f.Address.StateAbbr())
            .RuleFor(a => a.Country, _ => Country.UnitedStates)
            .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
            .RuleFor(a => a.Coordinates, f =>
            {
                double lat = f.Address.Latitude();
                double lon = f.Address.Longitude();
                ErrorOr<Coordinates> coordResult = Coordinates.Create(lat, lon);
                return coordResult.IsError ? null : coordResult.Value;
            });

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate();
    }
}
