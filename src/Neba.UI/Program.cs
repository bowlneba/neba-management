using System.Diagnostics;
using Neba.UI.Components;
using Neba.UI.Infrastructure;
using Neba.UI.Services;
using Serilog;
using Serilog.Debugging;
using SerilogTracing;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfiguration();

var serilogger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

Log.Logger = serilogger;
SelfLog.Enable(message => Debug.WriteLine(message));

using var _ = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests()
    .TraceTo(serilogger);

builder.Host.UseSerilog();

try
{
    builder.Services.AddMudBlazor();

#if !DEBUG
builder.Configuration.AddKeyVault();

#endif

    builder.Services.AddServices();

    builder.Services.AddApplicationInsightsTelemetry();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

#if !DEBUG
    app.UseAzureAppConfiguration();
#endif

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    await app.RunAsync();
}
#pragma warning disable CA1031
catch (Exception ex)
{
    serilogger.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}