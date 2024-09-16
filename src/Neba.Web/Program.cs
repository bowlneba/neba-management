using Refit;
using Neba.Web.Components;
using Neba.Web.Services.NebaApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var nebaApiOptions = builder.Configuration.GetSection(NebaApiOptions.SectionName).Get<NebaApiOptions>()
    ?? throw new InvalidOperationException($"Cannot read {NebaApiOptions.SectionName} from appsettings");

builder.Services.AddRefitClient<INebaApiV1>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{nebaApiOptions.BaseUrl}/v1"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Neba.Web.Client._Imports).Assembly);

await app.RunAsync();
