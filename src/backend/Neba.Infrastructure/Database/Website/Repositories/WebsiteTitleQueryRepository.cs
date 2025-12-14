using Microsoft.EntityFrameworkCore;
using Neba.Application.Bowlers.BowlerTitles;

namespace Neba.Infrastructure.Database.Website.Repositories;

internal sealed class WebsiteTitleQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteTitleQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BowlerTitleDto>> ListTitlesAsync(CancellationToken cancellationToken)
        => await _dbContext.Titles
            .AsNoTracking()
            .Select(title => new BowlerTitleDto
            {
                BowlerId = title.Bowler.Id,
                BowlerName = title.Bowler.Name.ToDisplayName(),
                TournamentMonth = title.Month,
                TournamentYear = title.Year,
                TournamentType = title.TournamentType
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BowlerTitleSummaryDto>> ListTitleSummariesAsync(CancellationToken cancellationToken)
        => await _dbContext.Titles
            .AsNoTracking()
            .GroupBy(title => new { title.Bowler.Id, title.Bowler.Name })
            .Select(group => new BowlerTitleSummaryDto
            {
                BowlerId = group.Key.Id,
                BowlerName = group.Key.Name.ToDisplayName(),
                TitleCount = group.Count()
            })
            .ToListAsync(cancellationToken);
}
