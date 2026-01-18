using Neba.Domain.Identifiers;
using Neba.Website.Contracts.Tournaments;

namespace Neba.Web.Server.Tournaments;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class TournamentMappingExtensions
{
    private static readonly Uri s_defaultThumbnailUri
        = new("/images/tournaments/default-logo.jpg", UriKind.Relative);

    extension(TournamentSummaryResponse response)
    {
        public TournamentSummaryViewModel ToViewModel()
            => new()
            {
                Id = response.Id,
                Name = response.Name,
                ThumbnailUrl = response.ThumbnailUrl ?? s_defaultThumbnailUri,
                BowlingCenterId = response.BowlingCenterId ?? BowlingCenterId.Empty,
                BowlingCenterName = response.BowlingCenterName ?? "TBD",
                StartDate = response.StartDate,
                EndDate = response.EndDate,
                TournamentType = response.TournamentType,
                PatternLengthCategory = response.PatternLengthCategory,
            };
    }
}
