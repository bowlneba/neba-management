using System.Diagnostics.CodeAnalysis;
using Neba.Application;
using Neba.Endpoints;
using Neba.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(config
    => config.ValidateOnBuild = true);

builder.Services.AddOpenApi();

builder.AddNebaEndpoints();

builder.AddSharedApplicationServices()
    .AddSharedInfrastructureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSharedInfrastructure();

app.UseNebaEndpoints();

await app.RunAsync();

/// <summary>
/// Represents the entry point for the Neba API.
/// </summary>
[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Program class needs to be public for the application entry point.")]
[SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "Program class needs to be public for the application entry point.")]
public partial class Program;