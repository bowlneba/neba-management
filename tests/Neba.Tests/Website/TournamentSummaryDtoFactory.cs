using Bogus;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Application.Tournaments;
using Neba.Website.Domain.BowlingCenters;

namespace Neba.Tests.Website;

public static class TournamentSummaryDtoFactory
{
    public static TournamentSummaryDto Create(
        TournamentId? id,
        string? name = null,
        Uri? thumbnailUrl = null,
        BowlingCenterId? bowlingCenterId = null,
        string? bowlingCenterName = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        TournamentType? tournamentType = null,
        PatternLengthCategory? patternLengthCategory = null)
        => new()
        {
            Id = id ?? TournamentId.New(),
            Name = name ?? "Sample Tournament",
            ThumbnailUrl = thumbnailUrl,
            BowlingCenterId = bowlingCenterId,
            BowlingCenterName = bowlingCenterName,
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            TournamentType = tournamentType ?? TournamentType.Singles,
            PatternLengthCategory = patternLengthCategory,
        };

    public static TournamentSummaryDto Bogus(
        IReadOnlyCollection<BowlingCenter> seededBowlingCenters,
        int? seed = null)
            => Bogus(seededBowlingCenters, 1, seed).Single();

    public static IReadOnlyCollection<TournamentSummaryDto> Bogus(
        IReadOnlyCollection<BowlingCenter> seededBowlingCenters,
        int count,
        int? seed = null)
    {
        var randomizer = new Randomizer(seed ?? Environment.TickCount);

        BowlingCenter bowlingCenter
            = seededBowlingCenters.ElementAt(randomizer.Int(0, seededBowlingCenters.Count - 1));

        return Bogus(bowlingCenter, count, seed);
    }

    public static IReadOnlyCollection<TournamentSummaryDto> Bogus(
        int count,
        int? seed = null)
    {
        IReadOnlyCollection<BowlingCenter> bowlingCenters = BowlingCenterFactory.Bogus(count * 10, seed);

        return Bogus(bowlingCenters, count, seed);
    }

    private static List<TournamentSummaryDto> Bogus(
        BowlingCenter bowlingCenter,
        int count,
        int? seed = null)
    {
        Faker<TournamentSummaryDto> faker = new Faker<TournamentSummaryDto>()
            .CustomInstantiator(faker =>
            {
                return new TournamentSummaryDto
                {
                    Id = TournamentId.New(),
                    Name = faker.Company.CompanyName() + " Tournament",
                    ThumbnailUrl = null,
                    BowlingCenterId = bowlingCenter.Id,
                    BowlingCenterName = bowlingCenter.Name,
                    StartDate = DateOnly.FromDateTime(faker.Date.Soon(30)),
                    EndDate = DateOnly.FromDateTime(faker.Date.Soon(35)),
                    TournamentType = faker.PickRandom(TournamentType.List.ToArray()),
                    PatternLengthCategory = faker.Random.Bool() ? faker.PickRandom(PatternLengthCategory.List.ToArray()) : null
                };
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
