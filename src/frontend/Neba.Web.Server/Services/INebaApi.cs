using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Refit;

namespace Neba.Web.Server.Services;

internal interface INebaApi
{
    [Get("/bowlers/titles/summary")]
    Task<Refit.ApiResponse<CollectionResponse<BowlerTitleSummaryResponse>>> GetBowlerTitlesSummaryAsync();

    [Get("/bowlers/titles")]
    Task<Refit.ApiResponse<CollectionResponse<BowlerTitleResponse>>> GetAllTitlesAsync();

    [Get("/bowlers/{bowlerId}/titles")]
    Task<Refit.ApiResponse<Contracts.ApiResponse<BowlerTitlesResponse>>> GetBowlerTitlesAsync(Guid bowlerId);
}
