using Ardalis.SmartEnum.SystemTextJson;
using Azure.Identity;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Neba.ServiceDefaults;
using Neba.Web.Server;
using Neba.Web.Server.BackgroundJobs;
using Neba.Web.Server.Documents;
using Neba.Web.Server.Maps;
using Neba.Web.Server.Notifications;
using Neba.Web.Server.Services;
using Refit;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Key Vault configuration if enabled
bool useKeyVault = builder.Configuration.GetValue("KeyVault:Enabled", false);
if (useKeyVault)
{
    string? vaultUrl = builder.Configuration["KeyVault:VaultUrl"];
    if (!string.IsNullOrEmpty(vaultUrl))
    {
        var vaultUri = new Uri(vaultUrl);
        builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
    }
}

builder.Services.AddOptions<NebaApiConfiguration>()
    .Bind(builder.Configuration.GetSection("NebaApi"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<NebaApiConfiguration>>().Value);

builder.Services.AddOptions<AzureMapsSettings>()
    .Bind(builder.Configuration.GetSection("AzureMaps"))
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<AzureMapsSettings>>().Value);

builder.Services.AddRefitClient<INebaWebsiteApi>(new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new SmartEnumNameConverter<Neba.Domain.Tournaments.TournamentType, int>(),
                    new SmartEnumValueConverter<Neba.Domain.Tournaments.TournamentType, int>(),
                    new SmartEnumNameConverter<Neba.Domain.Tournaments.PatternLengthCategory, int>(),
                    new SmartEnumValueConverter<Neba.Domain.Tournaments.PatternLengthCategory, int>()
                }
            })
})
    .ConfigureHttpClient((sp, client) =>
    {
        NebaApiConfiguration config = sp.GetRequiredService<NebaApiConfiguration>();
        client.BaseAddress = new Uri(config.BaseUrl);
    });

builder.Services.AddBackgroundJobs(builder.Configuration);

// API services
builder.Services.AddScoped<NebaWebsiteApiService>();

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

app.UseDefaultEndpoints();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Neba.Web.Client._Imports).Assembly);

app.UseBackgroundJobsDashboard();

// API endpoints for slide-over panel to fetch document content
app.MapGet("/api/documents/bylaws", async (NebaWebsiteApiService nebaWebsiteApiService) =>
{
    ErrorOr<DocumentViewModel<MarkupString>> result = await nebaWebsiteApiService.GetBylawsAsync();
    return result.IsError
        ? Results.Problem(detail: result.FirstError.Description, statusCode: 500)
        : Results.Ok(new
        {
            html = result.Value.Content.ToString(),
            metadata = result.Value.Metadata
        });
});

app.MapGet("/api/documents/tournaments/rules", async (NebaWebsiteApiService nebaWebsiteApiService) =>
{
    ErrorOr<DocumentViewModel<MarkupString>> result = await nebaWebsiteApiService.GetTournamentRulesAsync();

    return result.IsError
        ? Results.Problem(detail: result.FirstError.Description, statusCode: 500)
        : Results.Ok(new
        {
            html = result.Value.Content.ToString(),
            metadata = result.Value.Metadata
        });
});

await app.RunAsync();
