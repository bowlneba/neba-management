using Refit;
using Neba.Web.Components;
using Neba.Web.Services.NebaApi;
using Microsoft.Extensions.Options;
using Neba.Web.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.Configure<NebaApiOptions>(builder.Configuration.GetSection(NebaApiOptions.SectionName));

builder.Services.AddTransient<NebaApiAuthenticationDelegatingHandler>();

builder.Services.AddRefitClient<INebaApiV1>()
    .ConfigureHttpClient((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<NebaApiOptions>>().Value;
        client.BaseAddress = new Uri($"{options.BaseUrl}/v1");
    })
    .AddHttpMessageHandler<NebaApiAuthenticationDelegatingHandler>();

builder.AddOpenTelemetry();

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
