using Neba.Contracts.Website.Awards;

namespace Neba.Web.Server.History.Awards;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class AwardsMappingExtensions
{
    extension(HighAverageAwardResponse response)
    {
        public HighAverageAwardViewModel ToViewModel()
            => new()
            {
                Season = response.Season,
                BowlerName = response.BowlerName,
                Average = response.Average,
                GamesBowled = response.Games,
                TournamentsBowled = response.Tournaments
            };
    }
}
