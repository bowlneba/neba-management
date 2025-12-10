using Microsoft.EntityFrameworkCore;
using Neba.Application.Awards;
using Neba.Domain.Awards;

namespace Neba.Infrastructure.Database.Website.Repositories;

internal sealed class WebsiteAwardQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteAwardQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BowlerOfTheYearDto>> ListBowlerOfTheYearAwardsAsync(CancellationToken cancellationToken)
        => await _dbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.BowlerOfTheYear)
            .Select(award => new BowlerOfTheYearDto
            {
                Id = award.Id,
                BowlerId = award.BowlerId,
                BowlerName = award.Bowler.Name.ToDisplayName(),
                Season = award.Season,
                Category = award.BowlerOfTheYearCategory!
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<HighBlockAwardDto>> ListHigh5GameBlockAwardsAsync(CancellationToken cancellationToken)
        => await _dbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.High5GameBlock)
            .Select(award => new HighBlockAwardDto
            {
                Id = award.Id,
                BowlerName = award.Bowler.Name.ToDisplayName(),
                Season = award.Season,
                Score = award.HighBlockScore ?? -1,
            })
            .ToListAsync(cancellationToken);
}
