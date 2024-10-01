using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Neba.Infrastructure.Middleware;
using Neba.Application.Clock;
using Neba.Infrastructure.Clock;
using Neba.Infrastructure.Persistence;

namespace Neba.Infrastructure;

/// <summary>
/// Provides extension methods for adding and using infrastructure services.
/// </summary>
public static class InfrastructureDependencyInjection
{
    /// <summary>
    /// Adds shared infrastructure services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddAuditLogging();

        services.AddAuthorization();

        services.AddAuthentication(Authentication.ApiKeyAuthentication.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, Authentication.ApiKeyAuthentication>(Authentication.ApiKeyAuthentication.SchemeName, null);

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Adds audit logging services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddAuditLogging(this IServiceCollection services)
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

        return app;
    }
}