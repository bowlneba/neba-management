using System.Text.Json;
using Neba.Api.Endpoints.Website;
using Neba.Api.HealthChecks;
using Neba.Api.OpenApi;
using Neba.Application;
using Neba.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.ConfigureOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://app-bowlneba-web.azurewebsites.net", // Production web app
                "http://localhost:5100"   // Local development
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

WebApplication app = builder.Build();

app
    .UseOpenApi()
    .UseHealthChecks();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

// Future API endpoints will be added here
app.MapWebsiteEndpoints();

await app.RunAsync();

/// <summary>
/// Entry point for the Neba API application.
/// This partial class makes the implicit Program class accessible for integration tests.
/// </summary>
#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable CA1515
public partial class Program
#pragma warning restore CA1050
#pragma warning restore CA1515
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> class.
    /// </summary>
    protected Program()
    {
    }
}
