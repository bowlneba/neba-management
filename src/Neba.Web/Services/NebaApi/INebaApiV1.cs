using Refit;

namespace Neba.Web.Services.NebaApi;

internal interface INebaApiV1
{
    [Get("/weather")]
    Task<ApiResponse<WeatherForecast[]>> GetWeatherForecastsAsync();
}