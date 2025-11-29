using Microsoft.EntityFrameworkCore;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Bowlers;

namespace Neba.Infrastructure.Database.Website.Repositories;

internal sealed class WebsiteBowlerQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteBowlerQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BowlerTitlesSummaryDto>> GetAllBowlerTitlesSummaryAsync(CancellationToken cancellationToken)
        => await _dbContext.Bowlers
            .AsNoTracking()
            .Select(bowler => new BowlerTitlesSummaryDto
            {
                BowlerId = bowler.Id,
                BowlerName = bowler.Name.ToDisplayName(),
                TitleCount = bowler.Titles.Count
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BowlerTitleDto>> GetBowlerTitlesAsync(CancellationToken cancellationToken)
        => await _dbContext.Bowlers
            .AsNoTracking()
            .Where(bowler => bowler.Titles.Any())
            .SelectMany(bowler => bowler.Titles, (bowler, title) => new BowlerTitleDto
            {
                BowlerId = bowler.Id,
                BowlerName = bowler.Name.ToDisplayName(),
                TournamentMonth = title.Month,
                TournamentYear = title.Year,
                TournamentType = title.TournamentType
            })
            .ToListAsync(cancellationToken);

    public async Task<BowlerTitlesDto?> GetBowlerTitlesAsync(BowlerId bowlerId, CancellationToken cancellationToken)
        => await _dbContext.Bowlers
            .AsNoTracking()
            .Where(bowler => bowler.Id == bowlerId)
            .Select(bowler => new BowlerTitlesDto
            {
                BowlerId = bowler.Id,
                BowlerName = bowler.Name.ToDisplayName(),
                Titles = bowler.Titles
                    .OrderBy(title => title.Year)
                    .ThenBy(title => title.Month)
                    .ThenBy(title => title.TournamentType)
                    .Select(title => new TitleDto
                    {
                        Month = title.Month,
                        Year = title.Year,
                        TournamentType = title.TournamentType
                    })
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);
}
