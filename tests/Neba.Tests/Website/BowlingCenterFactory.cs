using Neba.Domain.Contact;
using Neba.Domain.Identifiers;
using Neba.Tests.Contact;
using Neba.Website.Domain.BowlingCenters;

namespace Neba.Tests.Website;

public static class BowlingCenterFactory
{
    public static BowlingCenter Create(
        BowlingCenterId? id = null,
        string? name = null,
        Address? address = null,
        PhoneNumber? phoneNumber = null,
        bool? isClosed = false,
        int? websiteId = null,
        int? applicationId = null
    )
        => new()
        {
            Id = id ?? BowlingCenterId.New(),
            Name = name ?? "Test Bowling Center",
            Address = address ?? AddressFactory.Create(),
            PhoneNumber = phoneNumber ?? PhoneNumberFactory.Create(),
            IsClosed = isClosed ?? false,
            WebsiteId = websiteId,
            ApplicationId = applicationId,
        };

    public static BowlingCenter Bogus(int? seed = null)
        => Bogus(count: 1, seed: seed).Single();

    public static IReadOnlyCollection<BowlingCenter> Bogus(
        int count,
        int? seed = null)
    {
        UniqueIdPool websiteIdPool = UniqueIdPool.Create(
            poolSize: 100000,
            baseOffset: 0,
            seed,
            probabilityOfValue: 0.5f);

        UniqueIdPool applicationIdPool = UniqueIdPool.Create(
            poolSize: 100000,
            baseOffset: 100_000_000,
            seed,
            probabilityOfValue: 0.5f);

        Bogus.Faker<BowlingCenter> faker = new Bogus.Faker<BowlingCenter>()
            .CustomInstantiator(f => new BowlingCenter
            {
                Id = BowlingCenterId.New(),
                Name = f.Company.CompanyName() + " Bowling",
                Address = AddressFactory.Bogus(seed),
                PhoneNumber = PhoneNumberFactory.Bogus(seed),
                IsClosed = f.Random.Bool(0.1f),
                WebsiteId = websiteIdPool.GetNext(),
                ApplicationId = applicationIdPool.GetNext()
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
