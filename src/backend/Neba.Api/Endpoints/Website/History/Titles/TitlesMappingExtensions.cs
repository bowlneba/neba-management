using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Contracts.History.Titles;

namespace Neba.Api.Endpoints.Website.History.Titles;

internal static class TitlesMappingExtensions
{
    extension(BowlerTitleDto dto)
    {
        public GetTitlesResponse ToResponseModel()
        {
            return new GetTitlesResponse
            {
                BowlerId = dto.BowlerId.Value,
                BowlerName = dto.BowlerName,
                TournamentMonth = dto.TournamentMonth,
                TournamentYear = dto.TournamentYear,
                TournamentType = dto.TournamentType.Name
            };
        }
    }
}
