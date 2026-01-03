using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.BowlingCenters;
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

        builder.HasIndex(tournament => tournament.Id)
            .IsUnique();

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

        builder.HasMany(tournament => tournament.Champions)
            .WithOne(champion => champion.Tournament)
            .HasForeignKey(ForeignKeyName)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
