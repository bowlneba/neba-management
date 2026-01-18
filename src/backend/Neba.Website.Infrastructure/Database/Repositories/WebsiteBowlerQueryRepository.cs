using Microsoft.EntityFrameworkCore;
using Neba.Domain.Identifiers;
using Neba.Website.Application.Bowlers;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.Tournaments;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteBowlerQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteBowlerQueryRepository
{

    public async Task<BowlerTitlesDto?> GetBowlerTitlesAsync(BowlerId bowlerId, CancellationToken cancellationToken)
    {
        List<BowlerTitleDto> titles = await dbContext.Bowlers
            .AsNoTracking()
            .Where(bowler => bowler.Id == bowlerId)
            .SelectMany(bowler => bowler.Titles.Select(tournament => new BowlerTitleDto
            {
                BowlerName = bowler.Name,
                BowlerId = bowler.Id,
                TournamentType = tournament.TournamentType,
                TournamentDate = tournament.EndDate,
            }))
            .OrderBy(title => title.TournamentDate)
            .ThenBy(title => title.TournamentType)
            .ToListAsync(cancellationToken);

        if (titles.Count == 0)
        {
            return null;
        }

        bool isInHallOfFame = await dbContext.HallOfFameInductions
            .AnyAsync(induction => induction.Bowler.Id == bowlerId, cancellationToken);

        return new BowlerTitlesDto
        {
            BowlerId = bowlerId,
            BowlerName = titles[0].BowlerName,
            HallOfFame = isInHallOfFame,
            Titles = titles.ConvertAll(title => new TitleDto
            {
                TournamentType = title.TournamentType,
                TournamentDate = title.TournamentDate,
            })
            .AsReadOnly()
        };
    }
}
