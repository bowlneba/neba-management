#pragma warning disable CA1031 // Do not catch general exception types - We intentionally catch all exceptions to convert to ErrorOr

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Neba.Domain.Identifiers;
using Neba.ServiceDefaults.Telemetry;
using Neba.Web.Server.BowlingCenters;
using Neba.Web.Server.Documents;
using Neba.Web.Server.HallOfFame;
using Neba.Web.Server.History.Awards;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Tournaments;
using Neba.Website.Contracts.Awards;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.BowlingCenters;
using Neba.Website.Contracts.Titles;
using Neba.Website.Contracts.Tournaments;
using Refit;

namespace Neba.Web.Server.Services;

internal class NebaWebsiteApiService(INebaWebsiteApi nebaApi)
{
    private const string ApiEndpointKey = "api.endpoint";
    private const string DurationKey = "api.duration_ms";
    private const string HttpStatusCodeKey = "http.status_code";
    private const string ErrorTypeKey = "error.type";

    private static readonly ActivitySource s_activitySource = new("Neba.Web.Server");
    private static readonly Meter s_meter = new("Neba.Web.Server");

    private static readonly Counter<long> s_apiCalls = s_meter.CreateCounter<long>(
        "neba.frontend.api.calls",
        description: "Number of frontend API calls");

    private static readonly Counter<long> s_apiErrors = s_meter.CreateCounter<long>(
        "neba.frontend.api.errors",
        description: "Number of frontend API errors");

    private static readonly Histogram<double> s_apiDuration = s_meter.CreateHistogram<double>(
        "neba.frontend.api.duration",
        unit: "ms",
        description: "Frontend API call duration");
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

    public async Task<ErrorOr<IReadOnlyCollection<BowlingCenterViewModel>>> GetBowlingCentersAsync()
    {
        ErrorOr<Contracts.CollectionResponse<BowlingCenterResponse>> result
            = await ExecuteApiCallAsync(nebaApi.GetBowlingCentersAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items
            .Select(dto => dto.ToViewModel())
            .OrderBy(dto => dto.Name)
            .ToList()
            .AsReadOnly();
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

    public async Task<ErrorOr<IReadOnlyCollection<HallOfFameInductionViewModel>>> GetHallOfFameInductionsAsync()
    {
        ErrorOr<Contracts.CollectionResponse<HallOfFameInductionResponse>> result
            = await ExecuteApiCallAsync(nebaApi.GetHallOfFameInductionsAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items
            .OrderByDescending(dto => dto.Year)
            .Select(dto => dto.ToViewModel())
            .ToList()
            .AsReadOnly();
    }

    public async Task<ErrorOr<DocumentViewModel<MarkupString>>> GetTournamentRulesAsync()
    {
        ErrorOr<Contracts.DocumentResponse<string>> result = await ExecuteApiCallAsync(nebaApi.GetTournamentRulesAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new DocumentViewModel<MarkupString>
        {
            Content = new MarkupString(result.Value.Content),
            Metadata = result.Value.Metadata
        };
    }

    public async Task<ErrorOr<DocumentViewModel<MarkupString>>> GetBylawsAsync()
    {
        ErrorOr<Contracts.DocumentResponse<string>> result = await ExecuteApiCallAsync(nebaApi.GetBylawsAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new DocumentViewModel<MarkupString>
        {
            Content = new MarkupString(result.Value.Content),
            Metadata = result.Value.Metadata
        };
    }

    public async Task<ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>>> GetFutureTournamentsAsync()
    {
        ErrorOr<Contracts.CollectionResponse<TournamentSummaryResponse>> result
            = await ExecuteApiCallAsync(nebaApi.GetFutureTournamentsAsync);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items
            .Select(dto => dto.ToViewModel())
            .OrderBy(t => t.StartDate)
            .ToList()
            .AsReadOnly();
    }

    public async Task<ErrorOr<IReadOnlyCollection<TournamentSummaryViewModel>>> GetTournamentsInAYearAsync(int year)
    {
        ErrorOr<Contracts.CollectionResponse<TournamentSummaryResponse>> result
            = await ExecuteApiCallAsync(() => nebaApi.GetTournamentsInAYearAsync(year));

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Items
            .Select(dto => dto.ToViewModel())
            .OrderBy(t => t.StartDate)
            .ToList()
            .AsReadOnly();
    }

    private static async Task<ErrorOr<T>> ExecuteApiCallAsync<T>(Func<Task<ApiResponse<T>>> apiCall)
    {
        string endpointName = typeof(T).Name;
        using Activity? activity = s_activitySource.StartActivity("frontend.api_call");

        activity?.SetCodeAttributes(endpointName, "Neba.Web.Server");
        activity?.SetTag(ApiEndpointKey, endpointName);

        long startTimestamp = Stopwatch.GetTimestamp();
        TagList apiTags = new() { { ApiEndpointKey, endpointName } };
        s_apiCalls.Add(1, apiTags);

        try
        {
            ApiResponse<T> response = await apiCall();
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            activity?.SetTag(DurationKey, durationMs);
            activity?.SetTag(HttpStatusCodeKey, (int)response.StatusCode);

            TagList durationTags = new()
            {
                { ApiEndpointKey, endpointName },
                { HttpStatusCodeKey, (int)response.StatusCode },
                { "api.success", response.IsSuccessStatusCode }
            };
            s_apiDuration.Record(durationMs, durationTags);

            if (!response.IsSuccessStatusCode)
            {
                TagList errorTags = new()
                {
                    { ApiEndpointKey, endpointName },
                    { HttpStatusCodeKey, (int)response.StatusCode },
                    { ErrorTypeKey, "HttpError" }
                };
                s_apiErrors.Add(1, errorTags);
                activity?.SetStatus(ActivityStatusCode.Error, response.ReasonPhrase);

                return ApiErrors.RequestFailed(response.StatusCode, response.ReasonPhrase);
            }

            if (response.Content is null)
            {
                TagList errorTags = new()
                {
                    { ApiEndpointKey, endpointName },
                    { HttpStatusCodeKey, (int)response.StatusCode },
                    { ErrorTypeKey, "NoContent" }
                };
                s_apiErrors.Add(1, errorTags);
                activity?.SetStatus(ActivityStatusCode.Error, "No content");

                return ApiErrors.RequestFailed(response.StatusCode, "No content returned from API");
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return response.Content;
        }
        catch (ApiException ex)
        {
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            TagList errorTags = new()
            {
                { ApiEndpointKey, endpointName },
                { HttpStatusCodeKey, (int)ex.StatusCode },
                { ErrorTypeKey, "ApiException" }
            };
            s_apiErrors.Add(1, errorTags);
            s_apiDuration.Record(durationMs, errorTags);

            activity?.SetTag(DurationKey, durationMs);
            activity?.SetExceptionTags(ex);

            return ApiErrors.RequestFailed(ex.StatusCode, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            TagList errorTags = new()
            {
                { ApiEndpointKey, endpointName },
                { ErrorTypeKey, "NetworkError" }
            };
            s_apiErrors.Add(1, errorTags);
            s_apiDuration.Record(durationMs, errorTags);

            activity?.SetTag(DurationKey, durationMs);
            activity?.SetExceptionTags(ex);

            return ApiErrors.NetworkError(ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            TagList errorTags = new()
            {
                { ApiEndpointKey, endpointName },
                { ErrorTypeKey, "Timeout" }
            };
            s_apiErrors.Add(1, errorTags);
            s_apiDuration.Record(durationMs, errorTags);

            activity?.SetTag(DurationKey, durationMs);
            activity?.SetExceptionTags(ex);

            return ApiErrors.Timeout();
        }
        catch (Exception ex)
        {
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            TagList errorTags = new()
            {
                { ApiEndpointKey, endpointName },
                { ErrorTypeKey, ex.GetErrorType() }
            };
            s_apiErrors.Add(1, errorTags);
            s_apiDuration.Record(durationMs, errorTags);

            activity?.SetTag(DurationKey, durationMs);
            activity?.SetExceptionTags(ex);

            return ApiErrors.Unexpected(ex.Message);
        }
    }
}
