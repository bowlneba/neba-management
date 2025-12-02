using Neba.Contracts.Website.Bowlers;

namespace Neba.Web.Server.History.Champions;

internal static class BowlerTitleMappingExtensions
{
    extension(BowlerTitleSummaryResponse response)
    {
        public BowlerTitleSummaryViewModel ToViewModel()
            => new()
            {
                BowlerId = response.BowlerId,
                BowlerName = response.BowlerName,
                TitleCount = response.TitleCount
            };
    }

    extension(BowlerTitleResponse response)
    {
        public BowlerTitleViewModel ToViewModel()
            => new()
            {
                BowlerId = response.BowlerId,
                BowlerName = response.BowlerName,
                TournamentMonth = response.TournamentMonth.Value,
                TournamentYear = response.TournamentYear,
                TournamentType = response.TournamentType,
                HallOfFame = false // Will be set later when we have Hall of Fame data
            };

        public TitleViewModel ToTitleViewModel()
            => new()
            {
                TournamentDate = $"{response.TournamentMonth.ToShortString()} {response.TournamentYear}",
                TournamentType = response.TournamentType
            };
    }

    extension(BowlerTitlesResponse response)
    {
        public BowlerTitlesViewModel ToViewModel()
            => new()
            {
                BowlerName = response.BowlerName,
                Titles = response.Titles
                    .OrderBy(title => title.Year)
                    .ThenBy(title => title.Month.Value)
                    .Select(title => new TitleViewModel
                    {
                        TournamentDate = $"{title.Month.ToShortString()} {title.Year}",
                        TournamentType = title.TournamentType
                    }).ToList()
            };
    }
}
