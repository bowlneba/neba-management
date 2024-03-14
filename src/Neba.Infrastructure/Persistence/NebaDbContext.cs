using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neba.Infrastructure.Persistence.Interceptors;

namespace Neba.Infrastructure.Persistence;

public abstract class NebaDbContext
    : DbContext
{
    private readonly ILoggerFactory _loggerFactory;

    protected NebaDbContext(DbContextOptions options, ILoggerFactory loggerFactory)
        : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        Debug.Assert(optionsBuilder != null, nameof(optionsBuilder) + " != null");
        optionsBuilder.AddInterceptors(new SlowQueryInterceptor(_loggerFactory.CreateLogger<SlowQueryInterceptor>()));

        base.OnConfiguring(optionsBuilder);
    }
}
