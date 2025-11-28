#pragma warning disable CA1031 // Do not catch general exception types - We intentionally catch all exceptions to convert to ErrorOr

using ErrorOr;
using Neba.Contracts;
using Neba.Contracts.History.Champions;
using Refit;

namespace Neba.Web.Server.Services;

internal class NebaApiService(INebaApi nebaApi)
{
    public Task<ErrorOr<CollectionResponse<GetBowlerTitleCountsResponse>>> GetBowlerTitleCountsAsync()
    {
        return ExecuteApiCallAsync(() => nebaApi.GetBowlerTitleCountsAsync());
    }

    public async Task<ErrorOr<GetBowlerTitlesResponse>> GetBowlerTitlesAsync(Guid bowlerId)
    {
        var result = await ExecuteApiCallAsync(() => nebaApi.GetBowlerTitlesAsync(bowlerId));

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Data;
    }

    private static async Task<ErrorOr<T>> ExecuteApiCallAsync<T>(Func<Task<Refit.ApiResponse<T>>> apiCall)
    {
        try
        {
            Refit.ApiResponse<T> response = await apiCall();

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
