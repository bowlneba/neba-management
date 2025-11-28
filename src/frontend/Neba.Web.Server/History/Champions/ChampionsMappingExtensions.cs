using Neba.Contracts.History.Champions;

namespace Neba.Web.Server.History.Champions;

internal static class ChampionsMappingExtensions
{
    extension(GetBowlerTitleCountsResponse getBowlerTitleCountsResponse)
    {
        public BowlerTitleCountViewModel ToViewModel()
            => new()
            {
                BowlerId = getBowlerTitleCountsResponse.BowlerId,
                BowlerName = getBowlerTitleCountsResponse.BowlerName,
                Titles = getBowlerTitleCountsResponse.TitleCount
            };
    }
}
