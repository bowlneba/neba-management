using Microsoft.EntityFrameworkCore;
using Neba.Website.Application.Tournaments;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteTournamentQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteTournamentQueryRepository
{
    public async Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsAfterDateAsync(DateOnly afterDate, CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .Where(tournament => tournament.StartDate >= afterDate)
            .OrderBy(tournament => tournament.StartDate)
            .Select(tournament => new TournamentSummaryDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                BowlingCenterId = tournament.BowlingCenterId,
                BowlingCenterName = tournament.BowlingCenter != null ? tournament.BowlingCenter.Name : null,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                TournamentType = tournament.TournamentType,
                PatternLengthCategory = tournament.LanePattern != null ? tournament.LanePattern.LengthCategory : null,
                ThumbnailUrl = null
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsInYearAsync(int year, CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .Where(tournament => tournament.StartDate.Year == year)
            .OrderBy(tournament => tournament.StartDate)
            .Select(tournament => new TournamentSummaryDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                BowlingCenterId = tournament.BowlingCenterId,
                BowlingCenterName = tournament.BowlingCenter != null ? tournament.BowlingCenter.Name : null,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                TournamentType = tournament.TournamentType,
                PatternLengthCategory = tournament.LanePattern != null ? tournament.LanePattern.LengthCategory : null,
                ThumbnailUrl = null
            })
            .ToListAsync(cancellationToken);
}
