using Bogus;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Contracts.Titles;

namespace Neba.Tests.Website;

public static class TitleResponseFactory
{
    public const string BowlerName = "Joe Bowler";
    public const string TournamentType = "Singles";

    public static TitleResponse Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        Month? tournamentMonth = null,
        int? tournamentYear = null,
        string? tournamentType = null)
        => new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? BowlerName,
            TournamentMonth = tournamentMonth ?? Month.January,
            TournamentYear = tournamentYear ?? 2023,
            TournamentType = tournamentType ?? TournamentType
        };

    public static TitleResponse Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<TitleResponse> Bogus(
        int count,
        int? seed = null)
    {
        Faker<TitleResponse> faker = new Faker<TitleResponse>()
            .RuleFor(b => b.BowlerId, _ => BowlerId.New())
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
