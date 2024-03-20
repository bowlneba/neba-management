using ErrorOr;

namespace Neba.UI.Services;

internal sealed class WeatherService
    : NebaApiService, IWeatherService
{
    public WeatherService(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
    }

    public async Task<ErrorOr<WeatherForecast[]>> GetForecastAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);

        var result = await GetAsync<WeatherForecast[]>("weather", cancellationToken);

        return result;
    }
}

internal interface IWeatherService
{
    Task<ErrorOr<WeatherForecast[]>> GetForecastAsync(CancellationToken cancellationToken);
}
