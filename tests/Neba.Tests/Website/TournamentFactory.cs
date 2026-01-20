using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class TournamentFactory
{
    public static Tournament Create(
        TournamentId? id = null,
        string? name = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        BowlingCenterId? bowlingCenterId = null,
        TournamentType? tournamentType = null,
        int? entries = null,
        LanePattern? lanePattern = null,
        int? applicationId = null,
        IReadOnlyCollection<TournamentFile>? files = null)
            => new()
            {
                Id = id ?? TournamentId.New(),
                Name = name ?? "Test Tournament",
                StartDate = startDate ?? new DateOnly(2024, 1, 15),
                EndDate = endDate ?? new DateOnly(2024, 1, 15),
                BowlingCenterId = bowlingCenterId,
                BowlingCenter = BowlingCenterFactory.Create(id: bowlingCenterId),
                TournamentType = tournamentType ?? TournamentType.Singles,
                LanePattern = lanePattern,
                EntryCount = entries ?? 50,
                ApplicationId = applicationId,
                Files = files ?? []
            };

    public static Tournament Create(
        TournamentId? id = null,
        string? name = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        BowlingCenter? bowlingCenter = null,
        TournamentType? tournamentType = null,
        int? entries = null,
        LanePattern? lanePattern = null,
        int? applicationId = null,
        IReadOnlyCollection<TournamentFile>? files = null
    )
    {
        BowlingCenter bowlingCenterPopulated = bowlingCenter ?? BowlingCenterFactory.Create();

        return new Tournament
        {
            Id = id ?? TournamentId.New(),
            Name = name ?? "Test Tournament",
            StartDate = startDate ?? new DateOnly(2024, 1, 15),
            EndDate = endDate ?? new DateOnly(2024, 1, 15),
            BowlingCenterId = bowlingCenterPopulated.Id,
            BowlingCenter = bowlingCenterPopulated,
            EntryCount = entries ?? 50,
            TournamentType = tournamentType ?? TournamentType.Singles,
            LanePattern = lanePattern,
            ApplicationId = applicationId,
            Files = files ?? []
        };
    }

    public static Tournament Bogus(IReadOnlyCollection<BowlingCenter> seedBowlingCenters, int? seed = null)
        => Bogus(1, seedBowlingCenters, seed).Single();

    public static IReadOnlyCollection<Tournament> Bogus(
        int count,
        IEnumerable<BowlingCenter> seedBowlingCenters,
        int? seed = null)
    {
        UniqueIdPool applicationIdPool = UniqueIdPool.Create(
            poolSize: 100000,
            baseOffset: 100_000_000,
            seed,
            probabilityOfValue: 0.5f);

        Bogus.Faker<Tournament> faker = new Bogus.Faker<Tournament>()
            .CustomInstantiator(f =>
            {
                DateOnly startDate = DateOnly.FromDateTime(
                    f.Date.Between(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local), new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Local)));
                DateOnly endDate = startDate.AddDays(f.Random.Int(0, 7));

                BowlingCenter bowlingCenter = f.PickRandom(seedBowlingCenters);

                return new Tournament
                {
                    Id = TournamentId.New(),
                    Name = f.Commerce.ProductName() + " Tournament",
                    StartDate = startDate,
                    EndDate = endDate,
                    BowlingCenterId = bowlingCenter.Id,
                    TournamentType = f.PickRandom(TournamentType.List.ToArray()),
                    LanePattern = f.Random.Bool(0.6f) ? LanePatternFactory.Bogus(seed) : null,
                    EntryCount = f.Random.Int(25, 300),
                    ApplicationId = applicationIdPool.GetNext()
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
