using System.Security.Cryptography;
using Neba.Application;
using Neba.Application.Clock;
using Neba.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSharedApplicationServices()
    .AddSharedInfrastructureServices();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSharedMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weather", () =>
{
    var forecast = Enumerable.Range(1, 10).Select(index =>
        new Neba.Api.WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            RandomNumberGenerator.GetInt32(-20, 55),
            summaries[RandomNumberGenerator.GetInt32(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/utcNow", (IDateTimeProvider dateTimeProvider) => Results.Ok(dateTimeProvider.UtcNow));

app.Run();