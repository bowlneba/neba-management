using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neba.Infrastructure.Persistence.Interceptors;

namespace Neba.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the Neba application.
/// </summary>
public abstract class NebaDbContext
    : DbContext
{
    private readonly int _slowQueryThresholdInMilliseconds;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NebaDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <param name="loggerFactory">The logger factory to create loggers.</param>
    /// <param name="config">The configuration to retrieve settings.</param>
    protected NebaDbContext(DbContextOptions options, ILoggerFactory loggerFactory, IConfiguration config)
        : base(options)
    {
        _loggerFactory = loggerFactory;
        _slowQueryThresholdInMilliseconds = config.GetValue<int>("Database:SlowQueryThresholdInMilliseconds");
    }

    /// <summary>
    /// Configures the database context options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new SlowQueryInterceptor(_loggerFactory.CreateLogger<SlowQueryInterceptor>(), _slowQueryThresholdInMilliseconds));

        base.OnConfiguring(optionsBuilder);
    }
}