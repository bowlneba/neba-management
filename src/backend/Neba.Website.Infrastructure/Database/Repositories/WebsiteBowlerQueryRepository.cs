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
        => await dbContext.Bowlers
            .AsNoTracking()
            .Where(bowler => bowler.Id == bowlerId)
            .Select(bowler => new BowlerTitlesDto
            {
                BowlerId = bowler.Id,
                BowlerName = bowler.Name,
                HallOfFame = bowler.HallOfFameInductions.Count > 0,
                Titles = bowler.Titles
                    .OrderBy(title => title.Tournament.EndDate)
                    .ThenBy(title => title.Tournament.TournamentType)
                    .Select(title => new TitleDto
                    {
                        TournamentDate = title.Tournament.EndDate,
                        TournamentType = title.Tournament.TournamentType
                    })
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);
}
