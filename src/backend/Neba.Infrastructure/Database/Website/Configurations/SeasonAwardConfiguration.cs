using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Awards;
using Neba.Domain.Identifiers;

namespace Neba.Infrastructure.Database.Website.Configurations;

internal sealed class SeasonAwardConfiguration
    : IEntityTypeConfiguration<SeasonAward>
{
    public void Configure(EntityTypeBuilder<SeasonAward> builder)
    {
        builder.ToTable("season_awards", WebsiteDbContext.DefaultSchema);
        builder.HasKey(seasonAward => seasonAward.Id);

        builder.HasIndex(seasonAward => seasonAward.Season);

        builder.Property(seasonAward => seasonAward.Id)
            .ValueGeneratedNever()
            .HasMaxLength(26)
            .IsFixedLength()
            .HasConversion<SeasonAwardId.EfCoreValueConverter>();

        builder.Property(seasonAward => seasonAward.AwardType)
            .HasConversion<SmartEnumConverter<SeasonAwardType, int>>()
            .IsRequired();

        builder.Property(seasonAward => seasonAward.Season)
            .HasColumnName("season")
            .HasMaxLength(9) // e.g., "2023", "2024-2025"
            .IsRequired();

        builder.Property(seasonAward => seasonAward.BowlerId)
            .HasConversion<BowlerId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(seasonAward => seasonAward.BowlerOfTheYearCategory)
            .HasConversion<SmartEnumConverter<BowlerOfTheYearCategory, int>>();

        builder.Property(seasonAward => seasonAward.HighBlockScore);

        builder.Property(seasonAward => seasonAward.Average)
            .HasPrecision(5, 2);

        builder.Property(seasonAward => seasonAward.SeasonTotalGames);

        builder.Property(seasonAward => seasonAward.Tournaments);
    }
}
