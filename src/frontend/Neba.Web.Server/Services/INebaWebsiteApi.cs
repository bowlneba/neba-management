using Neba.Contracts;
using Neba.Domain.Identifiers;
using Neba.Website.Contracts.Awards;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.BowlingCenters;
using Neba.Website.Contracts.Titles;
using Neba.Website.Contracts.Tournaments;
using Refit;

namespace Neba.Web.Server.Services;

internal interface INebaWebsiteApi
{
    [Get("/titles")]
    Task<Refit.ApiResponse<CollectionResponse<TitleResponse>>> GetAllTitlesAsync();

    [Get("/titles/summary")]
    Task<Refit.ApiResponse<CollectionResponse<TitleSummaryResponse>>> GetTitlesSummaryAsync();

    [Get("/bowlers/{bowlerId}/titles")]
    Task<Refit.ApiResponse<Contracts.ApiResponse<BowlerTitlesResponse>>> GetBowlerTitlesAsync(BowlerId bowlerId);

    [Get("/bowling-centers")]
    Task<Refit.ApiResponse<CollectionResponse<BowlingCenterResponse>>> GetBowlingCentersAsync();

    [Get("/awards/bowler-of-the-year")]
    Task<Refit.ApiResponse<CollectionResponse<BowlerOfTheYearResponse>>> GetBowlerOfTheYearAwardsAsync();

    [Get("/awards/high-block")]
    Task<Refit.ApiResponse<CollectionResponse<HighBlockAwardResponse>>> GetHighBlockAwardsAsync();

    [Get("/awards/high-average")]
    Task<Refit.ApiResponse<CollectionResponse<HighAverageAwardResponse>>> GetHighAverageAwardsAsync();

    [Get("/hall-of-fame")]
    Task<Refit.ApiResponse<CollectionResponse<HallOfFameInductionResponse>>> GetHallOfFameInductionsAsync();

    [Get("/tournaments/rules")]
    Task<Refit.ApiResponse<DocumentResponse<string>>> GetTournamentRulesAsync();

    [Get("/tournaments/future")]
    Task<Refit.ApiResponse<CollectionResponse<TournamentSummaryResponse>>> GetFutureTournamentsAsync();

    [Get("/tournaments/year/{year}")]
    Task<Refit.ApiResponse<CollectionResponse<TournamentSummaryResponse>>> GetTournamentsInAYearAsync(int year);

    [Get("/bylaws")]
    Task<Refit.ApiResponse<DocumentResponse<string>>> GetBylawsAsync();
}
