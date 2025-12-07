using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Bowlers;
using Neba.Domain.Bowlers.BowlerAwards;

namespace Neba.Infrastructure.Database.Website.Configurations;

internal sealed class BowlerOfTheYearConfiguration
    : IEntityTypeConfiguration<BowlerOfTheYear>
{
    public void Configure(EntityTypeBuilder<BowlerOfTheYear> builder)
    {
        builder.ToTable("bowler_of_the_years", WebsiteDbContext.DefaultSchema);
        builder.HasKey(bowlerOfTheYear => bowlerOfTheYear.Id);

        builder.HasIndex(bowlerOfTheYear => bowlerOfTheYear.Season);

        builder
            .Property(bowlerOfTheYear => bowlerOfTheYear.Id)
            .ValueGeneratedNever()
            .HasConversion<BowlerOfTheYearId.EfCoreValueConverter>();

        builder
            .Property(bowlerOfTheYear => bowlerOfTheYear.Season)
            .HasColumnName("season")
            .HasMaxLength(9) // e.g., "2023", "2024-2025"
            .IsRequired();

        builder
            .Property(bowlerOfTheYear => bowlerOfTheYear.Category)
            .HasConversion<SmartEnumConverter<BowlerOfTheYearCategory, int>>()
            .IsRequired();

        builder
            .Property(bowlerOfTheYear => bowlerOfTheYear.BowlerId)
            .HasConversion<BowlerId.EfCoreValueConverter>()
            .IsRequired();
    }
}
