using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.Titles;

namespace Neba.Website.Endpoints.Bowlers;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class TitlesMappingExtensions
{
    extension(BowlerTitleDto dto)
    {
        public TitleResponse ToResponseModel()
        {
            return new TitleResponse
            {
                BowlerId = dto.BowlerId,
                BowlerName = dto.BowlerName.ToDisplayName(),
                TournamentMonth = dto.TournamentMonth,
                TournamentYear = dto.TournamentYear,
                TournamentType = dto.TournamentType.Name
            };
        }
    }

    extension(BowlerTitlesDto dto)
    {
        public BowlerTitlesResponse ToResponseModel()
        {
            return new BowlerTitlesResponse
            {
                BowlerId = dto.BowlerId,
                BowlerName = dto.BowlerName.ToDisplayName(),
                Titles = dto.Titles.Select(title => new BowlerTitleResponse
                {
                    Month = title.Month,
                    Year = title.Year,
                    TournamentType = title.TournamentType.Name
                }).ToList()
            };
        }
    }

    extension(BowlerTitleSummaryDto dto)
    {
        public TitleSummaryResponse ToResponseModel()
        {
            return new TitleSummaryResponse
            {
                BowlerId = dto.BowlerId,
                BowlerName = dto.BowlerName.ToDisplayName(),
                TitleCount = dto.TitleCount
            };
        }
    }
}
