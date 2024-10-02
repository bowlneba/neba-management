using Neba.Application;
using Neba.Endpoints;
using Neba.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Specify the assembly containing your endpoints
builder.Services.AddNebaEndpoints();

builder.Services
    .AddSharedApplicationServices()
    .AddSharedInfrastructureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSharedInfrastructure();

app.UseNebaEndpoints();

await app.RunAsync();
