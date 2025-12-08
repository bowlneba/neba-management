using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts.Website.Bowlers;
using Neba.Contracts.Website.Titles;

namespace Neba.Api.Endpoints.Website.Bowlers;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class TitlesMappingExtensions
{
    extension(BowlerTitleDto dto)
    {
        public Contracts.Website.Titles.TitleResponse ToResponseModel()
        {
            return new Contracts.Website.Titles.TitleResponse
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
