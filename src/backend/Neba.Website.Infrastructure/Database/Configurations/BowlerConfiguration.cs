using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Bowlers;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class BowlerConfiguration
    : IEntityTypeConfiguration<Bowler>
{
    internal const string ForeignKeyName = "bowler_id";

    public void Configure(EntityTypeBuilder<Bowler> builder)
    {
        builder.ToTable("bowlers", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder
            .Property(bowler => bowler.Id)
            .IsUlid<BowlerId, BowlerId.EfCoreValueConverter>();

        builder.HasIndex(bowler => bowler.Id)
            .IsUnique();

        builder
            .OwnsOne(bowler => bowler.Name, nameBuilder =>
            {
                nameBuilder.HasIndex(name => new { name.LastName, name.FirstName });

                nameBuilder
                    .Property(name => name.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(20)
                    .IsRequired();

                nameBuilder
                    .Property(name => name.MiddleName)
                    .HasColumnName("middle_name")
                    .HasMaxLength(20);

                nameBuilder
                    .Property(name => name.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsRequired();

                nameBuilder
                    .Property(name => name.Suffix)
                    .HasColumnName("suffix")
                    .HasMaxLength(5);

                nameBuilder
                    .Property(name => name.Nickname)
                    .HasColumnName("nickname")
                    .HasMaxLength(30);
            });

        builder.Property(bowler => bowler.WebsiteId)
            .ValueGeneratedNever();

        builder.HasIndex(bowler => bowler.WebsiteId)
            .IsUnique()
            .AreNullsDistinct();

        builder.Property(bowler => bowler.ApplicationId)
            .ValueGeneratedNever();

        builder.HasIndex(bowler => bowler.ApplicationId)
            .IsUnique()
            .AreNullsDistinct();

        builder.HasMany(bowler => bowler.Titles)
            .WithOne(title => title.Bowler)
            .HasForeignKey(ForeignKeyName)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(bowler => bowler.SeasonAwards)
            .WithOne(seasonAward => seasonAward.Bowler)
            .HasForeignKey(ForeignKeyName)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(bowler => bowler.HallOfFameInductions)
            .WithOne(induction => induction.Bowler)
            .HasForeignKey(ForeignKeyName)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
