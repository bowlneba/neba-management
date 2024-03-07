using Neba.UI.Components;
using Neba.UI.Services;

#if !DEBUG
using Neba.UI.Infrastructure;
#endif

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddMudBlazor();

#if DEBUG

builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

#else

builder.Configuration.AddKeyVault();

#endif

builder.Services.AddServices();

builder.Services.AddApplicationInsightsTelemetry();

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
