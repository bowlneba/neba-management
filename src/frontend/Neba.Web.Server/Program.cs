using Microsoft.Extensions.Options;
using Neba.Web.Server;
using Neba.Web.Server.Services;
using Refit;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<NebaApiConfiguration>()
    .Bind(builder.Configuration.GetSection("NebaApi"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<NebaApiConfiguration>>().Value);

builder.Services.AddRefitClient<INebaApi>(new RefitSettings
    {
        ContentSerializer = new SystemTextJsonContentSerializer(
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            })
    })
    .ConfigureHttpClient((sp, client) =>
    {
        NebaApiConfiguration config = sp.GetRequiredService<NebaApiConfiguration>();
        client.BaseAddress = new Uri(config.BaseUrl);
    });

// Notification services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<AlertService>();

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
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Neba.Web.Client._Imports).Assembly);

await app.RunAsync();
