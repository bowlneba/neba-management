using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Neba.Infrastructure.Middleware;
using Neba.Application.Clock;
using Neba.Infrastructure.Clock;
using Neba.Infrastructure.Persistence;
using System.Diagnostics.CodeAnalysis;
using Neba.Infrastructure.Diagnostics;

namespace Neba.Infrastructure;

/// <summary>
/// Provides extension methods for adding and using infrastructure services.
/// </summary>
public static class InfrastructureDependencyInjection
{
    /// <summary>
    /// Adds shared infrastructure services to the specified <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The web application builder to add the services to.</param>
    /// <returns>The web application builder with the added services.</returns>
    public static WebApplicationBuilder AddSharedInfrastructureServices([NotNull] this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddAuditLogging();

        builder.Services.AddAuthorization();

        builder.Services.AddAuthentication(Authentication.ApiKeyAuthentication.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, Authentication.ApiKeyAuthentication>(Authentication.ApiKeyAuthentication.SchemeName, null);

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.AddOpenTelemetry();

        return builder;
    }

    /// <summary>
    /// Adds audit logging services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The service collection with the added services.</returns>
    private static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        services.AddKeyedScoped<List<AuditEntry>>("Audit", (_, _) => []);

        return services;
    }

    /// <summary>
    /// Configures the application to use shared infrastructure services.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder with the configured services.</returns>
    public static IApplicationBuilder UseSharedInfrastructure(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseOpenTelemetry();

        return app;
    }
}