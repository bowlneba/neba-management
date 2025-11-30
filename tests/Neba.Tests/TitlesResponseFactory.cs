using Bogus;
using Neba.Contracts.Website.Bowlers;

namespace Neba.Tests;

public static class TitlesResponseFactory
{
    public const string TournamentType = "Open Championship";

    public static TitleResponse Create(
        Month? month = null,
        int? year = null,
        string? tournamentType = null)
        => new()
        {
            Month = month ?? Month.January,
            Year = year ?? 2020,
            TournamentType = tournamentType ?? TournamentType
        };

    public static TitleResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<TitleResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<TitleResponse> faker = new Faker<TitleResponse>()
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
