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
        LanePattern? lanePattern = null,
        int? websiteId = null,
        int? applicationId = null,
        IReadOnlyCollection<Title>? champions = null)
            => new()
            {
                Id = id ?? TournamentId.New(),
                Name = name ?? "Test Tournament",
                StartDate = startDate ?? new DateOnly(2024, 1, 15),
                EndDate = endDate ?? new DateOnly(2024, 1, 15),
                BowlingCenterId = bowlingCenterId,
                TournamentType = tournamentType ?? TournamentType.Singles,
                LanePattern = lanePattern,
                WebsiteId = websiteId,
                ApplicationId = applicationId,
                Champions = champions ?? []
            };

    public static Tournament Bogus(IReadOnlyCollection<BowlingCenter> seedBowlingCenters, int? seed = null)
        => Bogus(1, seedBowlingCenters, seed).Single();

    public static IReadOnlyCollection<Tournament> Bogus(
        int count,
        IEnumerable<BowlingCenter> seedBowlingCenters,
        int? seed = null)
    {
        // Create pools of unique IDs to avoid collisions across tests
        UniqueIdPool websiteIdPool = UniqueIdPool.Create(
            poolSize: 100000,
            baseOffset: 0,
            seed,
            probabilityOfValue: 0.5f);

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

                var bowlingCenter = f.PickRandom(seedBowlingCenters);

                return new Tournament
                {
                    Id = TournamentId.New(),
                    Name = f.Commerce.ProductName() + " Tournament",
                    StartDate = startDate,
                    EndDate = endDate,
                    BowlingCenterId = bowlingCenter.Id,
                    TournamentType = f.PickRandom(TournamentType.List.ToArray()),
                    LanePattern = f.Random.Bool(0.6f) ? LanePatternFactory.Bogus(seed) : null,
                    WebsiteId = websiteIdPool.GetNext(),
                    ApplicationId = applicationIdPool.GetNext(),
                    Champions = []
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
