using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Neba.Application;
using Neba.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Specify the assembly containing your endpoints
builder.Services.AddFastEndpoints(options
    => options.Assemblies = [Neba.Api.Endpoints.AssemblyMarker.Assembly])
    .AddAuthorization() // this moves to infrastructure
    .AddAuthentication(Neba.Api.Infrastructure.Authentication.ApiKeyAuthentication.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, Neba.Api.Infrastructure.Authentication.ApiKeyAuthentication>(Neba.Api.Infrastructure.Authentication.ApiKeyAuthentication.SchemeName, null);

builder.Services
    .AddSharedApplicationServices()
    .AddSharedInfrastructureServices();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSharedInfrastructure();

app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";

    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
});

await app.RunAsync();
