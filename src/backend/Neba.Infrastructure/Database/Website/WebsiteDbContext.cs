using Microsoft.EntityFrameworkCore;
using Neba.Domain.Bowlers;
using Neba.Infrastructure.Database.Website.Configurations;

namespace Neba.Infrastructure.Database.Website;

internal sealed class WebsiteDbContext(DbContextOptions<WebsiteDbContext> options)
    : NebaDbContext(options)
{
    internal const string DefaultSchema = "website";

    public DbSet<Bowler> Bowlers
        => Set<Bowler>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);

        modelBuilder.ApplyConfiguration(new BowlerConfiguration());
        modelBuilder.ApplyConfiguration(new TitleConfiguration());
        modelBuilder.ApplyConfiguration(new BowlerOfTheYearConfiguration());
    }
}
