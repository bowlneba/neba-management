
using Neba.Contracts.Website.Bowlers;

namespace Neba.Web.Server.History.Champions;

internal static class ChampionsMappingExtensions
{
    extension(GetBowlerTitlesSummaryResponse response)
    {
        public BowlerTitleSummaryViewModel ToViewModel()
            => new()
            {
                BowlerId = response.BowlerId,
                BowlerName = response.BowlerName,
                Titles = response.TitleCount
            };
    }

    extension(GetTitleResponse response)
    {
        public TitlesViewModel ToViewModel()
            => new()
            {
                TournamentDate = $"{response.TournamentMonth.ToShortString()} {response.TournamentYear}",
                TournamentType = response.TournamentType
            };
    }

    extension(GetBowlerTitlesResponse response)
    {
        public BowlerTitlesViewModel ToViewModel()
            => new()
            {
                BowlerName = response.BowlerName,
                Titles = response.Titles
                    .OrderBy(title => title.Year)
                    .ThenBy(title => title.Month.Value)
                    .Select(title => new TitlesViewModel
                    {
                        TournamentDate = $"{title.Month.ToShortString()} {title.Year}",
                        TournamentType = title.TournamentType
                    }).ToList()
            };
    }
}
