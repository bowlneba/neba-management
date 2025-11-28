using Neba.Contracts;
using Neba.Contracts.History.Champions;
using Refit;

namespace Neba.Web.Server;

internal interface INebaApi
{
    [Get("/history/champions")]
    Task<Refit.ApiResponse<CollectionResponse<GetBowlerTitleCountsResponse>>> GetBowlerTitleCountsAsync();

    [Get("/history/champions/{bowlerId}")]
    Task<Refit.ApiResponse<Contracts.ApiResponse<GetBowlerTitlesResponse>>> GetBowlerTitlesAsync(Guid bowlerId);
}
