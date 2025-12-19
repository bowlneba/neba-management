using Bogus;
using Neba.Domain.Identifiers;
using Neba.Web.Server.History.Champions;

namespace Neba.Tests;

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
            .RuleFor(vm => vm.BowlerId, _ => BowlerId.New())
            .RuleFor(vm => vm.BowlerName, f => f.Name.FullName())
            .RuleFor(vm => vm.TournamentMonth, f => f.Random.Int(1, 12))
            .RuleFor(vm => vm.TournamentYear, f => f.Date.Past(70).Year)
            .RuleFor(vm => vm.TournamentType, f => f.PickRandom("Singles", "Doubles", "Team", "All Events"))
            .RuleFor(vm => vm.HallOfFame, f => f.Random.Bool(0.1f));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
