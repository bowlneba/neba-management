using System.Security.Cryptography;
using Neba.Api;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

string[] summaries =
[
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
];

app.MapGet("/weather", (IConfiguration config) =>
    {
        int count = config.GetValue<int>("Weather");

        WeatherForecast[] forecast = [.. Enumerable.Range(1, count).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    RandomNumberGenerator.GetInt32(-20, 55),
                    summaries[RandomNumberGenerator.GetInt32(summaries.Length)]
                ))];

        return forecast;
    })
    .WithName("GetWeatherForecast");

await app.RunAsync();
