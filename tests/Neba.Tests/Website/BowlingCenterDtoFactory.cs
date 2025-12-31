using System.Globalization;
using Bogus;
using Neba.Domain.Contact;
using Neba.Website.Application.BowlingCenters;

namespace Neba.Tests.Website;

public static class BowlingCenterDtoFactory
{
    public static BowlingCenterDto Create(
        string? name = null,
        string? street = null,
        string? unit = null,
        string? city = null,
        UsState? state = null,
        string? zipCode = null,
        string? phoneNumber = null,
        string? extension = null,
        double? latitude = null,
        double? longitude = null)
            => new()
            {
                Name = name ?? "Test Bowling Center",
                Street = street ?? "123 Main St",
                Unit = unit,
                City = city ?? "Some City",
                State = state ?? UsState.NewYork,
                ZipCode = zipCode ?? "11747",
                PhoneNumber = phoneNumber ?? "(631) 555-0123",
                Extension = extension,
                Latitude = latitude ?? 40.7128,
                Longitude = longitude ?? -74.0060,
            };

    public static BowlingCenterDto Bogus(int? seed = null)
        => Bogus(count: 1, seed: seed).Single();

    public static IReadOnlyCollection<BowlingCenterDto> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlingCenterDto> faker = new Faker<BowlingCenterDto>()
            .CustomInstantiator(f => new BowlingCenterDto
            {
                Name = f.Company.CompanyName() + " Bowling",
                Street = f.Address.StreetAddress(),
                Unit = f.Random.Bool(0.1f) ? f.Address.SecondaryAddress() : null,
                City = f.Address.City(),
                State = UsState.FromValue(f.Address.StateAbbr()),
                ZipCode = f.Address.ZipCode("#####"),
                PhoneNumber = f.Phone.PhoneNumber("##########"),
                Extension = f.Random.Bool(0.2f) ? f.Random.Number(100, 999).ToString(CultureInfo.InvariantCulture) : null,
                Latitude = f.Address.Latitude(),
                Longitude = f.Address.Longitude(),
                
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
