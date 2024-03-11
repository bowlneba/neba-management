using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.FeatureManagement;
using Neba.Api;
using Neba.Application;
using Neba.Application.Clock;
using Neba.Infrastructure;
using Serilog;
using Serilog.Debugging;
using SerilogTracing;

var builder = WebApplication.CreateBuilder(args);

var serilogger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

Log.Logger = serilogger;
SelfLog.Enable(message => Debug.WriteLine(message));

using var _ = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests()
    .Instrument.SqlClientCommands()
    .TraceTo(serilogger);

using var loggerFactory = LoggerFactory.Create(options => options.AddSerilog(serilogger, true));

builder.Host.UseSerilog();

try
{
    // Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSharedApplicationServices()
        .AddSharedInfrastructureServices(builder.Configuration, loggerFactory.CreateLogger("Infrastructure"));

    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

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

    app.MapGet("/weather", (ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("GetWeatherForecast");
#pragma warning disable CA1848 // Use the LoggerMessage delegates
            logger.LogInformation("Get Weather Request");
#pragma warning restore CA1848 // Use the LoggerMessage delegates

            var forecast = Enumerable.Range(1, 10).Select(index =>
                    new WeatherForecast
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

    app.MapGet("/featureFlag", async (IFeatureManager featureManager) =>
    {
        var featureFlag = await featureManager.IsEnabledAsync("Test-Feature");

        return Results.Ok($"Test-Feature enabled: {featureFlag}");
    });

    await app.RunAsync();
}
#pragma warning disable CA1031
catch (Exception ex)
{
    serilogger.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}