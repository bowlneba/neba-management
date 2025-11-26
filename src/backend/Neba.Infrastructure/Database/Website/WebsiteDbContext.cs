using Microsoft.EntityFrameworkCore;
using Neba.Domain.Bowlers;
using Neba.Infrastructure.Database.Website.Configurations;

namespace Neba.Infrastructure.Database.Website;

internal sealed class WebsiteDbContext(DbContextOptions<WebsiteDbContext> options)
    : NebaDbContext(options)
{
    public DbSet<Bowler> Bowlers
        => Set<Bowler>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BowlerConfiguration());
    }
}
