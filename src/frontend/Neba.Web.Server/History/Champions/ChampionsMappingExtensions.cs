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

    extension(GetBowlerTitlesResponse getBowlerTitlesResponse)
    {
        public BowlerTitlesViewModel ToViewModel()
            => new()
            {
                BowlerName = getBowlerTitlesResponse.BowlerName,
                Titles = getBowlerTitlesResponse.Titles
                    .OrderBy(title => title.Year)
                    .ThenBy(title => title.Month.Value)
                    .Select(title => new TitlesViewModel
                    {
                        TournamentDate = $"{title.Month.Name} {title.Year}",
                        TournamentType = title.TournamentType
                    }).ToList()
            };
    }
}
