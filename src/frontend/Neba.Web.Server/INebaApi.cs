using Neba.Contracts;
using Neba.Contracts.History.Titles;
using Refit;

namespace Neba.Web.Server;

internal interface INebaApi
{
    [Get("/history/titles")]
    Task<Refit.ApiResponse<CollectionResponse<GetBowlerTitlesResponse>>> GetBowlerTitlesAsync();

    [Get("/history/titles/{bowlerId}")]
    Task<Refit.ApiResponse<Contracts.ApiResponse<GetBowlerTitlesResponse>>> GetBowlerTitlesAsync(Guid bowlerId);
}
