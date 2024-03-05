using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using Neba.UI.Components;
using Neba.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopEnd;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

#region Key Vault

var kvUrl = builder.Configuration.GetValue<string>("KeyVault:Url") ?? throw new InvalidOperationException("KeyVault:Url is not set");

KeyClient keyClient = null!;

#if DEBUG

var kvClientId = builder.Configuration.GetValue<string>("KeyVault:ClientId");
var kvClientSecret = builder.Configuration.GetValue<string>("KeyVault:ClientSecret");
var kvTenantId = builder.Configuration.GetValue<string>("KeyVault:TenantId");

var credential = new ClientSecretCredential(kvTenantId, kvClientId, kvClientSecret);
keyClient = new(new Uri(kvUrl), credential);

#else

var credential = new ManagedIdentityCredential();
keyClient = new(new Uri(kvUrl), credential);

#endif

builder.Configuration.AddAzureKeyVault(new SecretClient(new Uri(kvUrl), credential), new KeyVaultSecretManager());

#endregion

#region Neba Api

builder.Services.AddOptions<NebaApiOptions>()
    .BindConfiguration(NebaApiOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient(NebaApiService._serviceName, (services, client) =>
{
    var options = services.GetRequiredService<IOptions<NebaApiOptions>>().Value;
    client.BaseAddress = options.BaseUrl;
});

builder.Services.AddScoped<IWeatherService, WeatherService>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
