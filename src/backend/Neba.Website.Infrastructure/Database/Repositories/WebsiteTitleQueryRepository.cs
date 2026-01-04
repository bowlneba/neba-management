using Microsoft.EntityFrameworkCore;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteTitleQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteTitleQueryRepository
{

    public async Task<IReadOnlyCollection<BowlerTitleDto>> ListTitlesAsync(CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .SelectMany(tournament => tournament.ChampionIds,
                (tournament, bowlerId) => new { tournament, bowlerId })
            .Join(dbContext.Bowlers,
                tc => tc.bowlerId,
                bowler => bowler.Id,
                (tc, bowler) => new BowlerTitleDto
                {
                    BowlerId = bowler.Id,
                    BowlerName = bowler.Name,
                    TournamentDate = tc.tournament.EndDate,
                    TournamentType = tc.tournament.TournamentType
                })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BowlerTitleSummaryDto>> ListTitleSummariesAsync(CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .SelectMany(tournament => tournament.ChampionIds)
            .GroupBy(bowlerId => bowlerId)
            .Join(dbContext.Bowlers,
                group => group.Key,
                bowler => bowler.Id,
                (group, bowler) => new BowlerTitleSummaryDto
                {
                    BowlerId = bowler.Id,
                    BowlerName = bowler.Name,
                    TitleCount = group.Count(),
                    HallOfFame = bowler.HallOfFameInductions.Any()
                })
            .ToListAsync(cancellationToken);
}
