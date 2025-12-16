using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Bowlers;

namespace Neba.Infrastructure.Database.Website.Configurations;

internal sealed class BowlerConfiguration
    : IEntityTypeConfiguration<Bowler>
{
    public void Configure(EntityTypeBuilder<Bowler> builder)
    {
        builder.ToTable("bowlers", WebsiteDbContext.DefaultSchema);
        builder.HasKey(bowler => bowler.Id);

        builder
            .Property(bowler => bowler.Id)
            .ValueGeneratedNever()
            .HasMaxLength(26)
            .IsFixedLength()
            .HasConversion<BowlerId.EfCoreValueConverter>();

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
            .HasForeignKey(title => title.BowlerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(bowler => bowler.SeasonAwards)
            .WithOne(seasonAward => seasonAward.Bowler)
            .HasForeignKey(seasonAward => seasonAward.BowlerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
