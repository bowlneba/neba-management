#pragma warning disable CA1031 // Do not catch general exception types - We intentionally catch all exceptions to convert to ErrorOr

using System.Collections.ObjectModel;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Neba.Domain.Identifiers;
using Neba.Web.Server.History.Awards;
using Neba.Web.Server.History.Champions;
using Neba.Website.Contracts.Awards;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.Titles;
using Refit;

namespace Neba.Web.Server.Services;

internal class NebaApiService(INebaApi nebaApi)
{
    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitleSummaryViewModel>>> GetTitlesSummaryAsync()
    {
        ErrorOr<Contracts.CollectionResponse<TitleSummaryResponse>> result = await ExecuteApiCallAsync(() => nebaApi.GetTitlesSummaryAsync());

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items.Select(dto => dto.ToViewModel()).ToList().AsReadOnly();
    }

    public async Task<ErrorOr<BowlerTitlesViewModel>> GetBowlerTitlesAsync(BowlerId bowlerId)
    {
        ErrorOr<Contracts.ApiResponse<BowlerTitlesResponse>> result = await ExecuteApiCallAsync(() => nebaApi.GetBowlerTitlesAsync(bowlerId));

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Data.ToViewModel();
    }

    public async Task<ErrorOr<IReadOnlyCollection<TitlesByYearViewModel>>> GetTitlesByYearAsync()
    {
        ErrorOr<Contracts.CollectionResponse<TitleResponse>> result = await ExecuteApiCallAsync(() => nebaApi.GetAllTitlesAsync());

        if (result.IsError)
        {
            return result.Errors;
        }

        var titles = result.Value.Items.Select(dto => dto.ToViewModel()).ToList();

        ReadOnlyCollection<TitlesByYearViewModel> titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .OrderByDescending(group => group.Key)
            .Select(g => new TitlesByYearViewModel
            {
                Year = g.Key,
                Titles = g.OrderByDescending(t => t.TournamentMonth)
                          .ThenBy(t => t.TournamentType)
                          .ToList()
                          .AsReadOnly()
            })
            .ToList()
            .AsReadOnly();

        return titlesByYear;
    }

    public async Task<ErrorOr<IReadOnlyCollection<BowlerOfTheYearByYearViewModel>>> GetBowlerOfTheYearAwardsAsync()
    {
        ErrorOr<Contracts.CollectionResponse<BowlerOfTheYearResponse>> result
            = await ExecuteApiCallAsync(nebaApi.GetBowlerOfTheYearAwardsAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items
            .GroupBy(dto => dto.Season)
            .OrderByDescending(group => group.Key)
            .Select(group => new BowlerOfTheYearByYearViewModel
            {
                Season = group.Key,
                WinnersByCategory = group
                    .Select(dto => new KeyValuePair<string, string>(dto.Category, dto.BowlerName))
                    .ToList()
                    .AsReadOnly()
            })
            .ToList()
            .AsReadOnly();
    }

    public async Task<ErrorOr<IReadOnlyCollection<HighBlockAwardViewModel>>> GetHighBlockAwardsAsync()
    {
        ErrorOr<Contracts.CollectionResponse<HighBlockAwardResponse>> result
            = await ExecuteApiCallAsync(nebaApi.GetHighBlockAwardsAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items
            .GroupBy(dto => dto.Season)
            .OrderByDescending(group => group.Key)
            .Select(group => new HighBlockAwardViewModel
            {
                Season = group.Key,
                Score = group.First().Score,
                Bowlers = group
                    .Select(dto => dto.BowlerName)
                    .Order()
                    .ToList()
                    .AsReadOnly()
            })
            .ToList()
            .AsReadOnly();
    }

    public async Task<ErrorOr<IReadOnlyCollection<HighAverageAwardViewModel>>> GetHighAverageAwardsAsync()
    {
        ErrorOr<Contracts.CollectionResponse<HighAverageAwardResponse>> result
            = await ExecuteApiCallAsync(nebaApi.GetHighAverageAwardsAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items
            .OrderByDescending(dto => dto.Season)
            .Select(dto => dto.ToViewModel())
            .ToList()
            .AsReadOnly();
    }

    public async Task<ErrorOr<MarkupString>> GetTournamentRulesAsync()
    {
        ErrorOr<string> result = await ExecuteApiCallAsync(nebaApi.GetTournamentRulesAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new MarkupString(result.Value);
    }

    public async Task<ErrorOr<MarkupString>> GetBylawsAsync()
    {
        ErrorOr<string> result = await ExecuteApiCallAsync(nebaApi.GetBylawsAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new MarkupString(result.Value);
    }

    private static async Task<ErrorOr<T>> ExecuteApiCallAsync<T>(Func<Task<ApiResponse<T>>> apiCall)
    {
        try
        {
            ApiResponse<T> response = await apiCall();

            if (!response.IsSuccessStatusCode)
            {
                return ApiErrors.RequestFailed(response.StatusCode, response.ReasonPhrase);
            }

            if (response.Content is null)
            {
                return ApiErrors.RequestFailed(response.StatusCode, "No content returned from API");
            }

            return response.Content;
        }
        catch (ApiException ex)
        {
            // Refit API exceptions
            return ApiErrors.RequestFailed(ex.StatusCode, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            // Network-related errors (DNS failures, connection issues, etc.)
            return ApiErrors.NetworkError(ex.Message);
        }
        catch (TaskCanceledException)
        {
            // Timeout or cancellation
            return ApiErrors.Timeout();
        }
        catch (Exception ex)
        {
            // Unexpected errors
            return ApiErrors.Unexpected(ex.Message);
        }
    }
}
