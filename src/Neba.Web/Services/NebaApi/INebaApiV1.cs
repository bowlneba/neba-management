using Refit;

namespace Neba.Web.Services.NebaApi;

internal interface INebaApiV1
{
    [Get("/v1/weatherforecast")]
    Task<WeatherForecast[]> GetWeatherForecastsAsync();
}