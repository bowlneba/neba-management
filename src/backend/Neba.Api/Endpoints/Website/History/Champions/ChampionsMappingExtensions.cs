using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Contracts.History.Champions;

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
}
