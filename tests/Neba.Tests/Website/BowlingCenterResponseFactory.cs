using System.Globalization;
using Bogus;
using Neba.Domain.Contact;
using Neba.Website.Contracts.BowlingCenters;

namespace Neba.Tests.Website;

public static class BowlingCenterResponseFactory
{
    public static BowlingCenterResponse Create(
        string? name = null,
        string? street = null,
        string? unit = null,
        string? city = null,
        UsState? state = null,
        string? zipCode = null,
        string? phoneNumber = null,
        string? phoneExtension = null,
        double? latitude = null,
        double? longitude = null,
        bool? isClosed = null)
            => new()
            {
                Name = name ?? "Test Bowling Center",
                Street = street ?? "123 Main St",
                Unit = unit,
                City = city ?? "Some City",
                State = state ?? UsState.NewYork,
                ZipCode = zipCode ?? "11747",
                PhoneNumber = phoneNumber ?? "16315550123",
                PhoneExtension = phoneExtension,
                Latitude = latitude ?? 40.7128,
                Longitude = longitude ?? -74.0060,
                IsClosed = isClosed ?? false,
            };

    public static BowlingCenterResponse Bogus(int? seed = null)
        => Bogus(count: 1, seed: seed).Single();

    public static IReadOnlyCollection<BowlingCenterResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlingCenterResponse> faker = new Faker<BowlingCenterResponse>()
            .CustomInstantiator(f => new BowlingCenterResponse
            {
                Name = f.Company.CompanyName() + " Bowling",
                Street = f.Address.StreetAddress(),
                Unit = f.Random.Bool(0.1f) ? f.Address.SecondaryAddress() : null,
                City = f.Address.City(),
                State = UsState.FromValue(f.Address.StateAbbr()),
                ZipCode = f.Address.ZipCode("#####"),
                PhoneNumber = f.Phone.PhoneNumber("##########"),
                PhoneExtension = f.Random.Bool(0.2f) ? f.Random.Number(100, 999).ToString(CultureInfo.InvariantCulture) : null,
                Latitude = f.Address.Latitude(),
                Longitude = f.Address.Longitude(),
                IsClosed = f.Random.Bool(0.1f),
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
