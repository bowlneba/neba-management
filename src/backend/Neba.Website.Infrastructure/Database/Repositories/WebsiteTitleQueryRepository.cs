using Microsoft.EntityFrameworkCore;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteTitleQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteTitleQueryRepository
{

    public async Task<IReadOnlyCollection<BowlerTitleDto>> ListTitlesAsync(CancellationToken cancellationToken)
        => await dbContext.Titles
            .AsNoTracking()
            .Select(title => new BowlerTitleDto
            {
                BowlerId = title.Bowler.Id,
                BowlerName = title.Bowler.Name,
                TournamentMonth = title.Month,
                TournamentYear = title.Year,
                TournamentType = title.TournamentType
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BowlerTitleSummaryDto>> ListTitleSummariesAsync(CancellationToken cancellationToken)
        => await dbContext.Titles
            .AsNoTracking()
            .GroupBy(title => title.Bowler.Id)
            .Select(group => new BowlerTitleSummaryDto
            {
                BowlerId = group.Key,
                BowlerName = group.First().Bowler.Name,
                TitleCount = group.Count(),
                HallOfFame = group.First().Bowler.HallOfFameInductions.Any()
            })
            .ToListAsync(cancellationToken);
}
