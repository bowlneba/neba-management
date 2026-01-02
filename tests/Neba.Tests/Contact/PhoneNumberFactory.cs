using Bogus;
using ErrorOr;
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
            .RuleFor(p => p.CountryCode, _ => "1")
            .RuleFor(p => p.Number, f =>
            {
                // Generate valid area code (first digit 2-9, not N11)
                string areaCode;
                do
                {
                    areaCode = f.Random.Int(200, 999).ToString(System.Globalization.CultureInfo.InvariantCulture);
                } while (areaCode[1] == '1' && areaCode[2] == '1'); // Skip N11 codes

                string exchange = f.Random.Int(200, 999).ToString(System.Globalization.CultureInfo.InvariantCulture);
                string lineNumber = f.Random.Int(0, 9999).ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
                return $"{areaCode}{exchange}{lineNumber}";
            })
            .RuleFor(p => p.Extension, f => f.Random.Bool(0.2f) ? f.Random.Int(1, 9999).ToString(System.Globalization.CultureInfo.InvariantCulture) : null);

        if (seed.HasValue)
        {
            faker = faker.UseSeed(seed.Value);
        }

        return faker.Generate();
    }
}
