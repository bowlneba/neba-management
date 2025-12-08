using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Neba.Contracts.Website.Titles;
using Refit;

namespace Neba.Web.Server.Services;

internal interface INebaApi
{
    [Get("/titles")]
    Task<Refit.ApiResponse<CollectionResponse<Contracts.Website.Titles.TitleResponse>>> GetAllTitlesAsync();

    [Get("/titles/summary")]
    Task<Refit.ApiResponse<CollectionResponse<TitleSummaryResponse>>> GetTitlesSummaryAsync();

    [Get("/bowlers/{bowlerId}/titles")]
    Task<Refit.ApiResponse<Contracts.ApiResponse<BowlerTitlesResponse>>> GetBowlerTitlesAsync(Guid bowlerId);
}
