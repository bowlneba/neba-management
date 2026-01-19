using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Neba.Api.ErrorHandling;
using Neba.Api.OpenApi;
using Neba.Infrastructure;
using Neba.ServiceDefaults;
using Neba.Website.Application;
using Neba.Website.Endpoints;
using Neba.Website.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.ConfigureOpenApi();

// this should go w/ the cache stuffs when it comes
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// this is here temporarily for hangfire logging and will be properly configured later with OTEL setup
builder.Services.AddLogging();

builder.Services
    .AddInfrastructure(builder.Configuration, [
        WebsiteApplicationAssemblyReference.Assembly
        ])
    .AddWebsiteApplication()
    .AddWebsiteInfrastructure(builder.Configuration);

builder.Services.AddErrorHandling();

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

app.UseExceptionHandler();

//authorization / authentication would go here

app
    .UseOpenApi()
    .UseDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable CORS
app.UseCors();

app
    .UseInfrastructure();

// Initialize background jobs after Hangfire is configured
app.Services.InitializeWebsiteBackgroundJobs();

// Future API endpoints will be added here
app.MapWebsiteEndpoints();

#if DEBUG

app.MapGet("/", () => "Neba API is running...");

#endif

app.MapGet("/debug/clear-cache", async (HybridCache cache) =>
{
    await cache.RemoveByTagAsync("*");

    return Results.Ok("Cache cleared.");
});

await app.RunAsync();
