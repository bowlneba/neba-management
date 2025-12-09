using Microsoft.EntityFrameworkCore;
using Neba.Application.Awards;

namespace Neba.Infrastructure.Database.Website.Repositories;

internal sealed class WebsiteAwardQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteAwardQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BowlerOfTheYearDto>> ListBowlerOfTheYearAwardsAsync(CancellationToken cancellationToken)
        => await _dbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == Domain.Awards.SeasonAwardType.BowlerOfTheYear)
            .Select(award => new BowlerOfTheYearDto
            {
                Id = award.Id,
                BowlerId = award.BowlerId,
                BowlerName = award.Bowler.Name.ToDisplayName(),
                Season = award.Season,
                Category = award.BowlerOfTheYearCategory!
            })
            .ToListAsync(cancellationToken);
}
