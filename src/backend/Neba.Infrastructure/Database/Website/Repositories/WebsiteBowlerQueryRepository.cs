using Microsoft.EntityFrameworkCore;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitleCounts;

namespace Neba.Infrastructure.Database.Website.Repositories;

internal sealed class WebsiteBowlerQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteBowlerQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BowlerTitleCountDto>> GetBowlerTitleCountsAsync(CancellationToken cancellationToken)
        => await _dbContext.Bowlers
            .Where(bowler => bowler.Titles.Any())
            .Select(bowler => new BowlerTitleCountDto
            {
                BowlerId = bowler.Id,
                BowlerName = bowler.Name.ToDisplayName(),
                TitleCount = bowler.Titles.Count
            })
            .ToListAsync(cancellationToken);
}
