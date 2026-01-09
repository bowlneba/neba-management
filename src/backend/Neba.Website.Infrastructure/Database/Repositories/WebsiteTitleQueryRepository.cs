using Microsoft.EntityFrameworkCore;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteTitleQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteTitleQueryRepository
{

    public async Task<IReadOnlyCollection<BowlerTitleDto>> ListTitlesAsync(CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .SelectMany(tournament => tournament.Champions,
                (tournament, bowler) => new BowlerTitleDto
                {
                    BowlerId = bowler.Id,
                    BowlerName = bowler.Name,
                    TournamentDate = tournament.EndDate,
                    TournamentType = tournament.TournamentType
                })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BowlerTitleSummaryDto>> ListTitleSummariesAsync(CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .SelectMany(tournament => tournament.Champions)
            .GroupBy(bowler => bowler.Id)
            .Select(group => new BowlerTitleSummaryDto
            {
                BowlerId = group.Key,
                BowlerName = group.First().Name,
                TitleCount = group.Count(),
                HallOfFame = group.First().HallOfFameInductions.Any()
            })
            .ToListAsync(cancellationToken);
}
