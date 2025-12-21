using Neba.Contracts;
using Neba.Domain.Identifiers;
using Neba.Website.Contracts.Awards;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.Titles;
using Refit;

namespace Neba.Web.Server.Services;

internal interface INebaApi
{
    [Get("/titles")]
    Task<Refit.ApiResponse<CollectionResponse<TitleResponse>>> GetAllTitlesAsync();

    [Get("/titles/summary")]
    Task<Refit.ApiResponse<CollectionResponse<TitleSummaryResponse>>> GetTitlesSummaryAsync();

    [Get("/bowlers/{bowlerId}/titles")]
    Task<Refit.ApiResponse<Contracts.ApiResponse<BowlerTitlesResponse>>> GetBowlerTitlesAsync(BowlerId bowlerId);

    [Get("/awards/bowler-of-the-year")]
    Task<Refit.ApiResponse<CollectionResponse<BowlerOfTheYearResponse>>> GetBowlerOfTheYearAwardsAsync();

    [Get("/awards/high-block")]
    Task<Refit.ApiResponse<CollectionResponse<HighBlockAwardResponse>>> GetHighBlockAwardsAsync();

    [Get("/awards/high-average")]
    Task<Refit.ApiResponse<CollectionResponse<HighAverageAwardResponse>>> GetHighAverageAwardsAsync();

    [Get("/tournaments/rules")]
    Task<Refit.ApiResponse<string>> GetTournamentRulesAsync();

    [Get("/bylaws")]
    Task<Refit.ApiResponse<string>> GetBylawsAsync();
}
