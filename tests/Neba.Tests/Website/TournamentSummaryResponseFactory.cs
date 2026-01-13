using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Website.Contracts.Tournaments;

namespace Neba.Tests.Website;

public static class TournamentSummaryResponseFactory
{
    public static TournamentSummaryResponse Create(
        TournamentId? id = null,
        string? name = null,
        Uri? thumbnailUrl = null,
        BowlingCenterId? bowlingCenterId = null,
        string? bowlingCenterName = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        TournamentType tournamentType = null!,
        PatternLengthCategory? patternLengthCategory = null)
    {
        return new TournamentSummaryResponse
        {
            Id = id ?? TournamentId.New(),
            Name = name ?? "Sample Tournament",
            ThumbnailUrl = thumbnailUrl,
            BowlingCenterId = bowlingCenterId ?? BowlingCenterId.New(),
            BowlingCenterName = bowlingCenterName,
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TournamentType = tournamentType ?? TournamentType.Singles,
            PatternLengthCategory = patternLengthCategory,
        };
    }
}
