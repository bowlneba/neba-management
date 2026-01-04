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
        List<BowlerTitleDto> titles = await (
            from tournament in dbContext.Tournaments.AsNoTracking()
            where tournament.ChampionIds.Contains(bowlerId)
            from bowler in dbContext.Bowlers.AsNoTracking()
            where bowler.Id == bowlerId
            orderby tournament.EndDate, tournament.TournamentType
            select new BowlerTitleDto
            {
                BowlerName = bowler.Name,
                BowlerId = bowler.Id,
                TournamentType = tournament.TournamentType,
                TournamentDate = tournament.EndDate,
            }
        ).ToListAsync(cancellationToken);

        if (titles.Count == 0)
        {
            return null;
        }

        bool isInHallOfFame = await dbContext.HallOfFameInductions
            .AsNoTracking()
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
