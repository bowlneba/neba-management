using Bogus;
using Neba.Domain.Identifiers;
using Neba.Web.Server.History.Champions;

namespace Neba.Tests.Website;

public static class BowlerTitleViewModelFactory
{
    public const string DefaultBowlerName = "John Doe";
    public const int DefaultTournamentMonth = 1;
    public const int DefaultTournamentYear = 2024;
    public const string DefaultTournamentType = "Singles";

    public static BowlerTitleViewModel Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        int? tournamentMonth = null,
        int? tournamentYear = null,
        string? tournamentType = null,
        bool? hallOfFame = null)
        => new()
        {
            BowlerId = bowlerId ?? BowlerId.New(),
            BowlerName = bowlerName ?? DefaultBowlerName,
            TournamentMonth = tournamentMonth ?? DefaultTournamentMonth,
            TournamentYear = tournamentYear ?? DefaultTournamentYear,
            TournamentType = tournamentType ?? DefaultTournamentType,
            HallOfFame = hallOfFame ?? false
        };

    public static BowlerTitleViewModel Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleViewModel> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerTitleViewModel> faker = new Faker<BowlerTitleViewModel>()
            .CustomInstantiator(f => new BowlerTitleViewModel
            {
                BowlerId = BowlerId.New(),
                BowlerName = f.Name.FullName(),
                TournamentMonth = f.Random.Int(1, 12),
                TournamentYear = f.Date.Past(70).Year,
                TournamentType = f.PickRandom("Singles", "Doubles", "Team", "All Events"),
                HallOfFame = f.Random.Bool(0.1f)
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
