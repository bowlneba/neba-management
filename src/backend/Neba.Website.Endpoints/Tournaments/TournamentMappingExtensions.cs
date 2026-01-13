using Neba.Website.Application.Tournaments;
using Neba.Website.Contracts.Tournaments;

namespace Neba.Website.Endpoints.Tournaments;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class TournamentMappingExtensions
{
    extension(TournamentSummaryDto dto)
    {
        public TournamentSummaryResponse ToResponseModel()
            => new()
            {
                Id = dto.Id,
                Name = dto.Name,
                ThumbnailUrl = dto.ThumbnailUrl,
                BowlingCenterId = dto.BowlingCenterId,
                BowlingCenterName = dto.BowlingCenterName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TournamentType = dto.TournamentType,
                PatternLengthCategory = dto.PatternLengthCategory,
            };
    }
}
