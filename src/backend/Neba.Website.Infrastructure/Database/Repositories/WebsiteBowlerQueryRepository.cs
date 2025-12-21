using Microsoft.EntityFrameworkCore;
using Neba.Domain.Identifiers;
using Neba.Website.Application.Bowlers;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.Tournaments;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteBowlerQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteBowlerQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

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
