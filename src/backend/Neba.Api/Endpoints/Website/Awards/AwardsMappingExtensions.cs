using Neba.Application.Bowlers.BowlerAwards;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts.Website.Awards;
using Neba.Contracts.Website.Bowlers;
using Neba.Contracts.Website.Titles;

namespace Neba.Api.Endpoints.Website.Awards;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class AwardsMappingExtensions
{
    extension(BowlerOfTheYearDto dto)
    {
        public BowlerOfTheYearResponse ToResponseModel()
        {
            return new BowlerOfTheYearResponse
            {
                BowlerName = dto.BowlerName,
                Season = dto.Season,
                Category = dto.Category.Name
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
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
                TitleCount = dto.TitleCount
            };
        }
    }
}
