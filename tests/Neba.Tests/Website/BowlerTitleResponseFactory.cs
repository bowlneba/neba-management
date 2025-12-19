using Bogus;
using Neba.Domain;
using Neba.Website.Contracts.Bowlers;

namespace Neba.Tests.Website;

public static class BowlerTitleResponseFactory
{
    public const string TournamentType = "Open Championship";

    public static BowlerTitleResponse Create(
        Month? month = null,
        int? year = null,
        string? tournamentType = null)
        => new()
        {
            Month = month ?? Month.January,
            Year = year ?? 2020,
            TournamentType = tournamentType ?? TournamentType
        };

    public static BowlerTitleResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitleResponse> faker = new Faker<BowlerTitleResponse>()
            .RuleFor(response => response.Month, f => f.PickRandom(Month.List.ToArray()))
            .RuleFor(response => response.Year, f => f.Date.Past(70).Year)
            .RuleFor(response => response.TournamentType, f => f.Lorem.Word());

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
