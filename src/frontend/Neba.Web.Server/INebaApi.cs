using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Refit;

namespace Neba.Web.Server;

internal interface INebaApi
{
    [Get("/bowlers/titles")]
    Task<Refit.ApiResponse<CollectionResponse<GetBowlerTitlesResponse>>> GetBowlerTitlesAsync();

    [Get("/bowlers/{bowlerId}/titles")]
    Task<Refit.ApiResponse<Contracts.ApiResponse<GetBowlerTitlesResponse>>> GetBowlerTitlesAsync(Guid bowlerId);
}
