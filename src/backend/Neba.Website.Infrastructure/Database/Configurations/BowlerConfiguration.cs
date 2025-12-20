using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Bowlers;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class BowlerConfiguration
    : IEntityTypeConfiguration<Bowler>
{
    internal const string ShadowForeignKeyName = "bowler_db_id";

    public void Configure(EntityTypeBuilder<Bowler> builder)
    {
        // set up shadow primary int key and update foreign keys to use it
        // make the BowlerId a unique index instead (and do the same for other entity configurations)

        builder.ToTable("bowlers", WebsiteDbContext.DefaultSchema);

        builder.Property<int>("db_id")
            .HasColumnName("db_id")
            .ValueGeneratedOnAdd();

        builder.HasKey("db_id");

        builder
            .Property(bowler => bowler.Id)
            .ValueGeneratedNever()
            .HasColumnName("bowler_id")
            .HasMaxLength(26)
            .IsFixedLength()
            .HasConversion<BowlerId.EfCoreValueConverter>();

        builder.HasIndex(bowler => bowler.Id)
            .IsUnique();

        builder
            .OwnsOne(bowler => bowler.Name, nameBuilder =>
            {
                nameBuilder
                    .HasIndex(name => new { name.LastName, name.FirstName });

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
            .HasFilter("\"website_id\" IS NOT NULL");

        builder.Property(bowler => bowler.ApplicationId)
            .ValueGeneratedNever();

        builder.HasIndex(bowler => bowler.ApplicationId)
            .IsUnique()
            .HasFilter("\"application_id\" IS NOT NULL");

        builder.HasMany(bowler => bowler.Titles)
            .WithOne(title => title.Bowler)
            .HasForeignKey(ShadowForeignKeyName)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(bowler => bowler.SeasonAwards)
            .WithOne(seasonAward => seasonAward.Bowler)
            .HasForeignKey(ShadowForeignKeyName)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
