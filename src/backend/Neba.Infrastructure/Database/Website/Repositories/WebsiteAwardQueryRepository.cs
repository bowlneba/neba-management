using Microsoft.EntityFrameworkCore;
using Neba.Application.Bowlers.BowlerAwards;

namespace Neba.Infrastructure.Database.Website.Repositories;

internal sealed class WebsiteAwardQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteAwardQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BowlerOfTheYearDto>> ListBowlerOfTheYearAwardsAsync(CancellationToken cancellationToken)
        => await _dbContext.BowlerOfTheYears
            .AsNoTracking()
            .Select(award => new BowlerOfTheYearDto
            {
                BowlerId = award.BowlerId,
                BowlerName = award.Bowler.Name.ToDisplayName(),
                Season = award.Season,
                Category = award.Category
            })
            .ToListAsync(cancellationToken);
}
