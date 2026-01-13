using Bogus;
using Neba.Domain.Contact;

namespace Neba.Tests.Contact;

public static class PhoneNumberFactory
{
    public const string ValidCountryCode = "1";
    public const string ValidNumber = "5551234567";
    public const string ValidExtension = "123";

    public static PhoneNumber Create(
        string? countryCode = null,
        string? number = null,
        string? extension = null)
            => new()
            {
                CountryCode = countryCode ?? ValidCountryCode,
                Number = number ?? ValidNumber,
                Extension = extension
            };

    public static PhoneNumber Bogus(int? seed = null)
    {
        Faker<PhoneNumber> faker = new Bogus.Faker<PhoneNumber>()
            .CustomInstantiator(faker =>
            {
                // Generate valid area code (first digit 2-9, not N11)
                string areaCode;
                do
                {
                    areaCode = faker.Random.Int(200, 999).ToString(System.Globalization.CultureInfo.InvariantCulture);
                } while (areaCode[1] == '1' && areaCode[2] == '1'); // Skip N11 codes

                string exchange = faker.Random.Int(200, 999).ToString(System.Globalization.CultureInfo.InvariantCulture);
                string lineNumber = faker.Random.Int(0, 9999).ToString("D4", System.Globalization.CultureInfo.InvariantCulture);

                return new PhoneNumber()
                {
                    CountryCode = "1",
                    Number = $"{areaCode}{exchange}{lineNumber}",
                    Extension = faker.Random.Bool(0.2f) ? faker.Random.Int(1, 9999).ToString(System.Globalization.CultureInfo.InvariantCulture) : null
                };
            });

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate();
    }
}
