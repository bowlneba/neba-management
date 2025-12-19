using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;

namespace Neba.Infrastructure.Database;

/// <summary>
/// Base <see cref="DbContext"/> for Neba infrastructure that applies common configuration
/// conventions and behaviors used across the application's database contexts.
/// </summary>
/// <param name="options">The options to be used by the <see cref="DbContext"/> base class.</param>
public abstract class NebaDbContext(DbContextOptions options)
    : DbContext(options)
{
    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder"/> with common behaviors used by
    /// all Neba database contexts.
    /// </summary>
    /// <param name="optionsBuilder">The options builder provided by the EF Core runtime.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="optionsBuilder"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This method enables:
    /// <list type="bullet">
    /// <item><description>PostgreSQL exception processing via <c>UseExceptionProcessor()</c></description></item>
    /// <item><description>Snake case naming convention for database identifiers</description></item>
    /// <item><description>Detailed errors to aid debugging; and sensitive data logging in debug builds only</description></item>
    /// </list>
    /// Sensitive data logging is explicitly enabled only in DEBUG to avoid leaking PII in production logs.
    /// </remarks>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        optionsBuilder
            .UseExceptionProcessor()
            .UseSnakeCaseNamingConvention();

        optionsBuilder.EnableDetailedErrors();

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    /// <summary>
    /// Configure model conventions used by EF Core for this context.
    /// </summary>
    /// <param name="configurationBuilder">The convention builder used to register custom conventions.</param>
    /// <remarks>
    /// Currently configures support for <c>SmartEnum</c> value conversions via the
    /// <c>SmartEnum.EFCore.ModelConfigurationBuilderExtensions.ConfigureSmartEnum</c> helper.
    /// </remarks>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureSmartEnum();
    }
}
