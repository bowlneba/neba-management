using Microsoft.Extensions.Options;
using Neba.Web.Server;
using Neba.Web.Server.Components;
using Refit;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<NebaApiConfiguration>()
    .Bind(builder.Configuration.GetSection("NebaApi"))
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<NebaApiConfiguration>>().Value);

builder.Services.AddRefitClient<INebaApi>()
    .ConfigureHttpClient((sp, client) =>
    {
        NebaApiConfiguration config = sp.GetRequiredService<NebaApiConfiguration>();
        client.BaseAddress = new Uri(config.BaseUrl);
    });

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

WebApplication app = builder.Build();

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
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Neba.Web.Client._Imports).Assembly);

await app.RunAsync();
