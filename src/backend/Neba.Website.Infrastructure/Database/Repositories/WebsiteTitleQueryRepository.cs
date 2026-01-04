using Microsoft.EntityFrameworkCore;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteTitleQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteTitleQueryRepository
{

    public async Task<IReadOnlyCollection<BowlerTitleDto>> ListTitlesAsync(CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .SelectMany(tournament => tournament.Champions)
            .Select(title => new BowlerTitleDto
            {
                BowlerId = title.Bowler.Id,
                BowlerName = title.Bowler.Name,
                TournamentDate = title.Tournament.EndDate,
                TournamentType = title.Tournament.TournamentType
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BowlerTitleSummaryDto>> ListTitleSummariesAsync(CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .SelectMany(tournament => tournament.Champions)
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
