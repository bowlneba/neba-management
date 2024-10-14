using System.Diagnostics;
using System.Security.Cryptography;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Neba.Infrastructure.Diagnostics;

namespace Neba.Endpoints;

#pragma warning disable CA1812
internal sealed class GetWeatherForecast
    : EndpointWithoutRequest<Results<Ok<IReadOnlyCollection<WeatherForecast>>, InternalServerError>>
{
    public override void Configure()
    {
        Get("weather");
        Version(1);
        AuthSchemes("ApiKey"); //this constant is defined in the ApiKeyAuthentication class, when infrastructure is created, look to leverage that if possible since this will reference application
    }

    public override async Task<Results<Ok<IReadOnlyCollection<WeatherForecast>>, InternalServerError>> ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(1000, ct);

        var random = RandomNumberGenerator.GetInt32(1, 10);

        Activity.Current?.SetTag("neba.api.random", random);
        ApplicationDiagnostics.WeatherRequestCounter.Add(1,
            new KeyValuePair<string, object?>("neba.api.random", random)); //this is a custom metric, it is not required, but it is a good example of how to create a custom metric

        if (random % 2 == 0)
        {
            ThrowError("An even number was rolled, here is an error");
        }

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    RandomNumberGenerator.GetInt32(-20, 55),
                    _summaries[RandomNumberGenerator.GetInt32(_summaries.Length)]
                ))
            .ToArray();

        return TypedResults.Ok((IReadOnlyCollection<WeatherForecast>)forecast);
    }

    private static readonly string[] _summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
}