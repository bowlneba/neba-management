using Refit;

namespace Neba.Web.Server;

internal interface INebaApi
{
    [Get("/weather")]
    Task<WeatherForecast[]> GetWeatherAsync();
}
