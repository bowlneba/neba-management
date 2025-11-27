using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Contracts.History.Champions;

namespace Neba.Api.Endpoints.Website.History.Champions;

internal static class ChampionsMappingExtensions
{
    extension(BowlerTitleCountDto dto)
    {
        public GetBowlerTitleCountsResponseModel ToResponseModel()
        {
            return new GetBowlerTitleCountsResponseModel
            {
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
                TitleCount = dto.TitleCount
            };
        }
    }
}
