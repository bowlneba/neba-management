using Microsoft.EntityFrameworkCore;
using Neba.Domain.Awards;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.BowlerOfTheYear;
using Neba.Website.Application.Awards.HallOfFame;
using Neba.Website.Application.Awards.HighAverage;
using Neba.Website.Application.Awards.HighBlock;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteAwardQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteAwardQueryRepository
{

    public async Task<IReadOnlyCollection<BowlerOfTheYearAwardDto>> ListBowlerOfTheYearAwardsAsync(CancellationToken cancellationToken)
        => await dbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.BowlerOfTheYear)
            .Select(award => new BowlerOfTheYearAwardDto
            {
                Id = award.Id,
                BowlerId = award.Bowler.Id,
                BowlerName = award.Bowler.Name,
                Season = award.Season,
                Category = award.BowlerOfTheYearCategory!
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<HighBlockAwardDto>> ListHigh5GameBlockAwardsAsync(CancellationToken cancellationToken)
        => await dbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.High5GameBlock)
            .Select(award => new HighBlockAwardDto
            {
                Id = award.Id,
                BowlerName = award.Bowler.Name,
                Season = award.Season,
                Score = award.HighBlockScore ?? -1
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<HighAverageAwardDto>> ListHighAverageAwardsAsync(CancellationToken cancellationToken)
    {
        List<HighAverageAwardDto> awards = await dbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.HighAverage)
            .Select(award => new HighAverageAwardDto
            {
                Id = award.Id,
                BowlerName = award.Bowler.Name,
                Season = award.Season,
                Average = award.Average ?? -1,
                Games = award.SeasonTotalGames,
                Tournaments = award.Tournaments
            })
            .ToListAsync(cancellationToken);

        if (awards.Any(award => award.Average == -1))
        {
            throw new InvalidOperationException("High Average Awards with missing average score found.");
        }

        return awards;
    }

    public async Task<IReadOnlyCollection<HallOfFameInductionDto>> ListHallOfFameInductionsAsync(CancellationToken cancellationToken)
        => await dbContext.HallOfFameInductions
            .AsNoTracking()
            .Select(induction => new HallOfFameInductionDto
            {
                Year = induction.Year,
                BowlerName = induction.Bowler.Name,
                Photo = induction.Photo,
                Categories = induction.Categories
            })
            .ToListAsync(cancellationToken);
}
