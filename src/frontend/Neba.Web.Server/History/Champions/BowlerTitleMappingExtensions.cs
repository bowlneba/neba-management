using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.Titles;

namespace Neba.Web.Server.History.Champions;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class BowlerTitleMappingExtensions
{
    extension(TitleSummaryResponse response)
    {
        public BowlerTitleSummaryViewModel ToViewModel()
            => new()
            {
                BowlerId = response.BowlerId,
                BowlerName = response.BowlerName,
                TitleCount = response.TitleCount,
                HallOfFame = response.HallOfFame
            };
    }

    extension(TitleResponse response)
    {
        public BowlerTitleViewModel ToViewModel()
            => new()
            {
                BowlerId = response.BowlerId,
                BowlerName = response.BowlerName,
                TournamentMonth = response.TournamentMonth.Value,
                TournamentYear = response.TournamentYear,
                TournamentType = response.TournamentType
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
                HallOfFame = response.HallOfFame,
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
