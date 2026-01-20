using Microsoft.EntityFrameworkCore;
using Neba.Domain;
using Neba.Website.Application.Tournaments;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteTournamentQueryRepository(
    WebsiteDbContext dbContext,
    ITournamentUrlBuilder urlBuilder)
    : IWebsiteTournamentQueryRepository
{
    public async Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsAfterDateAsync(DateOnly afterDate, CancellationToken cancellationToken)
    {
        var results = await dbContext.Tournaments
            .AsNoTracking()
            .Where(tournament => tournament.StartDate >= afterDate)
            .OrderBy(tournament => tournament.StartDate)
            .Select(tournament => new
            {
                tournament.Id,
                tournament.Name,
                tournament.BowlingCenterId,
                BowlingCenterName = tournament.BowlingCenter != null ? tournament.BowlingCenter.Name : null,
                tournament.StartDate,
                tournament.EndDate,
                tournament.TournamentType,
                PatternLengthCategory = tournament.LanePattern != null ? tournament.LanePattern.LengthCategory : null,
                LogoFile = tournament.Files
                    .Where(file => file.FileType == TournamentFileType.Logo)
                    .Select(file => file.File)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return results.ConvertAll(result => new TournamentSummaryDto
        {
            Id = result.Id,
            Name = result.Name,
            BowlingCenterId = result.BowlingCenterId,
            BowlingCenterName = result.BowlingCenterName,
            StartDate = result.StartDate,
            EndDate = result.EndDate,
            TournamentType = result.TournamentType,
            PatternLengthCategory = result.PatternLengthCategory,
            ThumbnailUrl = urlBuilder.BuildFileUrl(result.LogoFile)
        });
    }

    public async Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsInYearAsync(int year, CancellationToken cancellationToken)
    {
        var results = await dbContext.Tournaments
            .AsNoTracking()
            .Where(tournament => tournament.StartDate.Year == year)
            .OrderBy(tournament => tournament.StartDate)
            .Select(tournament => new
            {
                tournament.Id,
                tournament.Name,
                tournament.BowlingCenterId,
                BowlingCenterName = tournament.BowlingCenter != null ? tournament.BowlingCenter.Name : null,
                tournament.StartDate,
                tournament.EndDate,
                tournament.TournamentType,
                PatternLengthCategory = tournament.LanePattern != null ? tournament.LanePattern.LengthCategory : null,
                LogoFile = tournament.Files
                    .Where(file => file.FileType == TournamentFileType.Logo)
                    .Select(file => file.File)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return results.ConvertAll(result => new TournamentSummaryDto
        {
            Id = result.Id,
            Name = result.Name,
            BowlingCenterId = result.BowlingCenterId,
            BowlingCenterName = result.BowlingCenterName,
            StartDate = result.StartDate,
            EndDate = result.EndDate,
            TournamentType = result.TournamentType,
            PatternLengthCategory = result.PatternLengthCategory,
            ThumbnailUrl = urlBuilder.BuildFileUrl(result.LogoFile)
        });
    }
}
