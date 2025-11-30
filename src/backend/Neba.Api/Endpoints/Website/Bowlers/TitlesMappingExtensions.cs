using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts.Website.Bowlers;

namespace Neba.Api.Endpoints.Website.Bowlers;

internal static class TitlesMappingExtensions
{
    extension(BowlerTitleDto dto)
    {
        public BowlerTitleResponse ToResponseModel()
        {
            return new BowlerTitleResponse
            {
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
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
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
                Titles = dto.Titles.Select(title => new TitleResponse
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
        public BowlerTitleSummaryResponse ToResponseModel()
        {
            return new BowlerTitleSummaryResponse
            {
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
                TitleCount = dto.TitleCount
            };
        }
    }
}
