using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace Neba.Infrastructure.Database;

internal abstract class NebaDbContext(DbContextOptions options)
    : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseExceptionProcessor()
            .UseSnakeCaseNamingConvention();

        optionsBuilder.EnableDetailedErrors();

    #if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
    #endif
    }
}
