using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Contracts.History.Champions;
using Neba.Contracts.History.Titles;

namespace Neba.Api.Endpoints.Website.History.Champions;

internal static class ChampionsMappingExtensions
{
    extension(BowlerTitleCountDto dto)
    {
        public GetBowlerTitleCountsResponse ToResponseModel()
        {
            return new GetBowlerTitleCountsResponse
            {
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
                TitleCount = dto.TitleCount
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
