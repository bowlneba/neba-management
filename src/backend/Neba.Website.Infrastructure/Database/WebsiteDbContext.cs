using Microsoft.EntityFrameworkCore;
using Neba.Infrastructure.Database;
using Neba.Website.Domain.Awards;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;
using Neba.Website.Infrastructure.Database.Configurations;

namespace Neba.Website.Infrastructure.Database;

internal sealed class WebsiteDbContext(DbContextOptions<WebsiteDbContext> options)
    : NebaDbContext(options)
{
    internal const string DefaultSchema = "website";

    public DbSet<Bowler> Bowlers
        => Set<Bowler>();

    public DbSet<BowlingCenter> BowlingCenters
        => Set<BowlingCenter>();

    public DbSet<Tournament> Tournaments
        => Set<Tournament>();

    public DbSet<SeasonAward> SeasonAwards
        => Set<SeasonAward>();

    public DbSet<HallOfFameInduction> HallOfFameInductions
        => Set<HallOfFameInduction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);

        modelBuilder.ApplyConfiguration(new BowlerConfiguration());
        modelBuilder.ApplyConfiguration(new BowlingCenterConfiguration());
        modelBuilder.ApplyConfiguration(new TournamentConfiguration());
        modelBuilder.ApplyConfiguration(new TournamentFileConfiguration());
        modelBuilder.ApplyConfiguration(new SeasonAwardConfiguration());
        modelBuilder.ApplyConfiguration(new HallOfFameInductionConfiguration());
    }
}
