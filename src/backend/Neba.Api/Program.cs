using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Neba.Api.HealthChecks;
using Neba.Api.OpenApi;
using Neba.Infrastructure;
using Neba.Website.Application;
using Neba.Website.Endpoints;
using Neba.Website.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.ConfigureOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// this is here temporarily for hangfire logging, and will be properly configured later with OTEL setup
builder.Services.AddLogging();

// this is here temporarily until we configure actual caching with cache query instances
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024 * 10; // 10 MB
    options.MaximumKeyLength = 512;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(15)
    };
});

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddWebsiteApplication()
    .AddWebsiteInfrastructure(builder.Configuration);

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

//authorization / authentication would go here

app
    .UseOpenApi()
    .UseHealthChecks();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

app
    .UseInfrastructure();

// Initialize background jobs after Hangfire is configured
app.Services.InitializeWebsiteBackgroundJobs();

// Future API endpoints will be added here
app.MapWebsiteEndpoints();

await app.RunAsync();
