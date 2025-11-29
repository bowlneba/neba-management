using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts.Website.Bowlers;

namespace Neba.Api.Endpoints.Website.Bowlers;

internal static class TitlesMappingExtensions
{
    extension(BowlerTitleDto dto)
    {
        public GetTitlesResponse ToResponseModel()
        {
            return new GetTitlesResponse
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
        public GetBowlerTitlesResponse ToResponseModel()
        {
            return new GetBowlerTitlesResponse
            {
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
                Titles = dto.Titles.Select(title => new TitlesResponse
                {
                    Month = title.Month,
                    Year = title.Year,
                    TournamentType = title.TournamentType.Name
                }).ToList()
            };
        }
    }
}
