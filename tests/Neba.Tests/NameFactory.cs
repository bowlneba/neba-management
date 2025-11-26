using Neba.Domain.Bowlers;

namespace Neba.Tests;

public static class NameFactory
{
    public const string FirstName = "John";
    public const string LastName = "Doe";
    public const string MiddleInitial = "A";
    public const string Suffix = "Jr.";
    public const string Nickname = "Johnny";
    private static readonly string[] s_suffixes = ["Jr.", "Sr.", "III", "IV"];

    public static Name Create(
        string firstName = FirstName,
        string lastName = LastName,
        string? middleInitial = null,
        string? suffix = null,
        string? nickname = null
    )
        => new()
        {
            FirstName = firstName,
            LastName = lastName,
            MiddleInitial = middleInitial,
            Suffix = suffix,
            Nickname = nickname
        };

    public static Name Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<Name> Bogus(int count, int? seed = null)
    {
        Bogus.Faker<Name> faker = new Bogus.Faker<Name>()
            .RuleFor(n => n.FirstName, f => f.Person.FirstName)
            .RuleFor(n => n.LastName, f => f.Person.LastName)
            .RuleFor(n => n.MiddleInitial, f => f.Random.Bool(0.3f) ? f.Random.Char('A', 'Z').ToString() : null)
            .RuleFor(n => n.Suffix, f => f.Random.Bool(0.2f) ? f.PickRandom(s_suffixes) : null)
            .RuleFor(n => n.Nickname, f => f.Random.Bool(0.4f) ? f.Name.FirstName() : null);

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
