using Microsoft.EntityFrameworkCore;
using Neba.Website.Application.Tournaments;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteTournamentQueryRepository(WebsiteDbContext dbContext)
    : IWebsiteTournamentQueryRepository
{
    public async Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsAfterDateAsync(DateOnly afterDate, CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .Where(tournament => tournament.StartDate >= afterDate)
            .OrderBy(tournament => tournament.StartDate)
            .Select(tournament => tournament.ToTournamentSummaryDto())
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsInYearAsync(int year, CancellationToken cancellationToken)
        => await dbContext.Tournaments
            .AsNoTracking()
            .Where(tournament => tournament.StartDate.Year == year)
            .OrderBy(tournament => tournament.StartDate)
            .Select(tournament => tournament.ToTournamentSummaryDto())
            .ToListAsync(cancellationToken);
}
