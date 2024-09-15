using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Specify the assembly containing your endpoints
builder.Services.AddFastEndpoints(options
    => options.Assemblies = [Neba.WebApi.Endpoints.AssemblyMarker.Assembly]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseFastEndpoints();

await app.RunAsync();
