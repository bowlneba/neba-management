using Neba.Website.Application.Tournaments;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database;

#pragma warning disable S1144 // Extension methods are used in other projects
#pragma warning disable CA1034 // Nested types should not be visible

/// <summary>
/// Provides mapping extensions for Tournament entities.
/// </summary>
public static class TournamentMappingExtensions
{
    extension (Tournament tournament)
    {
        /// <summary>
        /// Maps a Tournament entity to a TournamentSummaryDto.
        /// </summary>
        public TournamentSummaryDto ToTournamentSummaryDto()
            => new()
            {
                Id = tournament.Id,
                Name = tournament.Name,
                BowlingCenterId = tournament.BowlingCenterId,
                BowlingCenterName = tournament.BowlingCenter != null ? tournament.BowlingCenter.Name : null,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                TournamentType = tournament.TournamentType,
                PatternLengthCategory = tournament.LanePattern != null ? tournament.LanePattern.LengthCategory : null,
                ThumbnailUrl = null // placeholder until TournamentDocuments are implemented
            };
    }
}
