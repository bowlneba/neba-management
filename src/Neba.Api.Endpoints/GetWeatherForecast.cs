using System.Security.Cryptography;
using FastEndpoints;

namespace Neba.Neba.Api.Endpoints;

#pragma warning disable CA1812
internal sealed class GetWeatherForecast
    : EndpointWithoutRequest<IReadOnlyCollection<WeatherForecast>>
{
    public override void Configure()
    {
        Get("weatherforecast");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    RandomNumberGenerator.GetInt32(-20, 55),
                    _summaries[RandomNumberGenerator.GetInt32(_summaries.Length)]
                ))
            .ToArray();

        await SendAsync(forecast, 200, ct);
    }

    private static readonly string[] _summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
}