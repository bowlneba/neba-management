using Neba.Application.Awards.BowlerOfTheYear;
using Neba.Application.Awards.HighAverage;
using Neba.Application.Awards.HighBlock;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts.Website.Awards;
using Neba.Contracts.Website.Bowlers;
using Neba.Contracts.Website.Titles;

namespace Neba.Api.Endpoints.Website.Awards;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class AwardsMappingExtensions
{
    extension(BowlerOfTheYearAwardDto dto)
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
                BowlerId = dto.BowlerId,
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
                BowlerId = dto.BowlerId,
                BowlerName = dto.BowlerName,
                TitleCount = dto.TitleCount
            };
        }
    }

    extension(HighBlockAwardDto dto)
    {
        public HighBlockAwardResponse ToResponseModel()
        {
            return new HighBlockAwardResponse
            {
                BowlerName = dto.BowlerName,
                Season = dto.Season,
                Score = dto.Score
            };
        }
    }

    extension(HighAverageAwardDto dto)
    {
        public HighAverageAwardResponse ToResponseModel()
        {
            return new HighAverageAwardResponse
            {
                BowlerName = dto.BowlerName,
                Season = dto.Season,
                Average = dto.Average,
                Games = dto.Games,
                Tournaments = dto.Tournaments
            };
        }
    }
}
