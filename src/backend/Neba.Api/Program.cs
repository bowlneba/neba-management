using System.Text.Json;
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

app
    .UseOpenApi()
    .UseHealthChecks();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

// Future API endpoints will be added here
app.MapWebsiteEndpoints();

await app.RunAsync();
