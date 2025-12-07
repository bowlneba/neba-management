using Bogus;
using Neba.Contracts.Website.Bowlers;

namespace Neba.Tests;

public static class BowlerTitleResponseFactory
{
    public const string BowlerName = "Joe Bowler";
    public const string TournamentType = "Singles";

    public static BowlerTitleResponse Create(
        Guid? bowlerId = null,
        string? bowlerName = null,
        Month? tournamentMonth = null,
        int? tournamentYear = null,
        string? tournamentType = null)
        => new()
        {
            BowlerId = bowlerId ?? Guid.NewGuid(),
            BowlerName = bowlerName ?? BowlerName,
            TournamentMonth = tournamentMonth ?? Month.January,
            TournamentYear = tournamentYear ?? 2023,
            TournamentType = tournamentType ?? TournamentType
        };

    public static BowlerTitleResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitleResponse> faker = new Faker<BowlerTitleResponse>()
            .RuleFor(b => b.BowlerId, f => f.Random.Guid())
            .RuleFor(b => b.BowlerName, f => f.Name.FullName())
            .RuleFor(b => b.TournamentMonth, f => f.PickRandom(Month.List.ToArray()))
            .RuleFor(b => b.TournamentYear, f => f.Random.Int(2000, 2025))
            .RuleFor(b => b.TournamentType, f => f.Lorem.Word());

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}