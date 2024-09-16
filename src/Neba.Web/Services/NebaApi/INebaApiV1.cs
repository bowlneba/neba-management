using Refit;

namespace Neba.Web.Services.NebaApi;

internal interface INebaApiV1
{
    [Get("/weatherforecast")]
    Task<WeatherForecast[]> GetWeatherForecastsAsync();
}