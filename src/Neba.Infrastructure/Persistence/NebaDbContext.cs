using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neba.Application.Clock;
using Neba.Infrastructure.Persistence.Interceptors;

namespace Neba.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the Neba application.
/// </summary>
public abstract class NebaDbContext
    : DbContext
{
    private readonly List<AuditEntry> _auditEntries;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly int _slowQueryThresholdInMilliseconds;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<NebaDbContext> _logger;

    /// <summary>
    /// Gets the schema for the database context.
    /// </summary>
    protected abstract string Schema { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NebaDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <param name="loggerFactory">The logger factory to create loggers.</param>
    /// <param name="auditEntries">The list of audit entries.</param>
    /// <param name="dateTimeProvider">The date and time provider.</param>
    /// <param name="config">The configuration to retrieve settings.</param>
    private protected NebaDbContext(
        DbContextOptions options,
        ILoggerFactory loggerFactory,
        List<AuditEntry> auditEntries,
        IDateTimeProvider dateTimeProvider,
        IConfiguration config)
        : base(options)
    {
        _auditEntries = auditEntries;
        _dateTimeProvider = dateTimeProvider;
        _loggerFactory = loggerFactory;
        _slowQueryThresholdInMilliseconds = config.GetValue<int>("Database:SlowQueryThresholdInMilliseconds");
        _logger = loggerFactory.CreateLogger<NebaDbContext>();
    }

    /// <summary>
    /// Configures the database context options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
    {
        _logger.LogConfiguringDbContext();

        optionsBuilder.AddInterceptors(new SlowQueryInterceptor(_loggerFactory.CreateLogger<SlowQueryInterceptor>(), _slowQueryThresholdInMilliseconds));
        optionsBuilder.AddInterceptors(new AuditInterceptor(_auditEntries, _dateTimeProvider, _loggerFactory.CreateLogger<AuditInterceptor>()));

        base.OnConfiguring(optionsBuilder);

        _logger.LogConfiguredDbContext();
    }

    /// <summary>
    /// Applies all configurations that implement <see cref="IEntityTypeConfiguration{TEntity}"/> in the assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder to apply configurations to.</param>
    protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
    {
        _logger.LogApplyingConfigurations();

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        base.OnModelCreating(modelBuilder);

        _logger.LogAppliedConfigurations();
    }

    /// <summary>
    /// Gets the DbSet for audit entries.
    /// </summary>
    public DbSet<AuditEntry> AuditEntries
        => Set<AuditEntry>();
}

internal static partial class NebaDbContextLogMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Configuring the DbContext")]
    public static partial void LogConfiguringDbContext(this ILogger<NebaDbContext> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Configured the DbContext")]
    public static partial void LogConfiguredDbContext(this ILogger<NebaDbContext> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Applying entity configurations")]
    public static partial void LogApplyingConfigurations(this ILogger<NebaDbContext> logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Applied entity configurations")]
    public static partial void LogAppliedConfigurations(this ILogger<NebaDbContext> logger);
}