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
        List<BowlerTitleDto> titles = await dbContext.Tournaments.AsNoTracking()
            .SelectMany(tournament => tournament.Champions)
            .Where(title => title.Bowler.Id == bowlerId)
            .OrderBy(title => title.Tournament.EndDate)
            .ThenBy(title => title.Tournament.TournamentType)
            .Select(title => new BowlerTitleDto
            {
                BowlerName = title.Bowler.Name,
                BowlerId = title.Bowler.Id,
                TournamentType = title.Tournament.TournamentType,
                TournamentDate = title.Tournament.EndDate,
            })
            .ToListAsync(cancellationToken);

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
