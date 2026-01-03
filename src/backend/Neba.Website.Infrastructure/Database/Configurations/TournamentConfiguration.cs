using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class TournamentConfiguration
    : IEntityTypeConfiguration<Tournament>
{
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

        builder.HasOne(tournament => tournament.BowlingCenter)
            .WithMany()
            .HasForeignKey(BowlingCenterConfiguration.ForeignKeyName)
            .HasPrincipalKey("domain_id")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
