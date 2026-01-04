using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class TournamentConfiguration
    : IEntityTypeConfiguration<Tournament>
{
    internal const string ForeignKeyName = "tournament_id";

    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("tournaments", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder.Property(tournament => tournament.Id)
            .IsUlid<TournamentId, TournamentId.EfCoreValueConverter>();

        builder.HasAlternateKey(tournament => tournament.Id);

        builder.Property(tournament => tournament.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tournament => tournament.StartDate)
            .IsRequired();

        builder.Property(tournament => tournament.EndDate)
            .IsRequired();

        builder.Property(tournament => tournament.TournamentType)
            .HasConversion<SmartEnumConverter<TournamentType, int>>()
            .IsRequired();

        builder.Property(tournament => tournament.WebsiteId)
            .HasColumnName("website_id")
            .ValueGeneratedNever();

        builder.HasIndex(tournament => tournament.WebsiteId)
            .IsUnique()
            .AreNullsDistinct();

        builder.Property(tournament => tournament.ApplicationId)
            .HasColumnName("application_id")
            .ValueGeneratedNever();

        builder.HasIndex(tournament => tournament.ApplicationId)
            .IsUnique()
            .AreNullsDistinct();

        builder.Property(tournament => tournament.BowlingCenterId)
            .IsUlid<BowlingCenterId, BowlingCenterId.EfCoreValueConverter>(BowlingCenterConfiguration.ForeignKeyName);

        builder.HasOne(tournament => tournament.BowlingCenter)
            .WithMany()
            .HasForeignKey(tournament => tournament.BowlingCenterId)
            .HasPrincipalKey(bowlingCenter => bowlingCenter.Id)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ComplexProperty(tournament => tournament.LanePattern, lanePatternBuilder =>
        {
            lanePatternBuilder.Property(lanePattern => lanePattern.LengthCategory)
                .HasConversion<SmartEnumConverter<PatternLengthCategory, int>>()
                .HasColumnName("lane_pattern_length");

            lanePatternBuilder.Property(lanePattern => lanePattern.RatioCategory)
                .HasConversion<SmartEnumConverter<PatternRatioCategory, int>>()
                .HasColumnName("lane_pattern_ratio");
        });

        // Ignore the public ChampionIds property - it's computed from Champions
        builder.Ignore(tournament => tournament.ChampionIds);

        // Configure many-to-many relationship between Tournament and Bowler
        builder.HasMany(tournament => tournament.Champions)
            .WithMany(bowler => bowler.Titles)
            .UsingEntity<Dictionary<string, object>>(
                "tournament_champions",
                j => j.HasOne<Bowler>()
                    .WithMany()
                    .HasForeignKey(BowlerConfiguration.ForeignKeyName)
                    .HasPrincipalKey(nameof(Bowler.Id)),
                j => j.HasOne<Tournament>()
                    .WithMany()
                    .HasForeignKey(ForeignKeyName)
                    .HasPrincipalKey(nameof(Tournament.Id)),
                j =>
                {
                    j.ToTable("tournament_champions", WebsiteDbContext.DefaultSchema);
                    j.HasKey(ForeignKeyName, BowlerConfiguration.ForeignKeyName);
                    // Index on bowler_id for FK lookup performance
                    j.HasIndex(BowlerConfiguration.ForeignKeyName);
                });
    }
}
