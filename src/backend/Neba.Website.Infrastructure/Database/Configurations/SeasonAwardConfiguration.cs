using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Awards;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Awards;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class SeasonAwardConfiguration
    : IEntityTypeConfiguration<SeasonAward>
{
    public void Configure(EntityTypeBuilder<SeasonAward> builder)
    {
        builder.ToTable("season_awards", WebsiteDbContext.DefaultSchema);

        builder.Property<int>("db_id")
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.HasKey("db_id");

        builder.Property(seasonAward => seasonAward.Id)
            .IsUlid<SeasonAwardId, SeasonAwardId.EfCoreValueConverter>();

        builder.Property(seasonAward => seasonAward.AwardType)
            .HasConversion<SmartEnumConverter<SeasonAwardType, int>>()
            .IsRequired();

        builder.Property(seasonAward => seasonAward.Season)
            .HasColumnName("season")
            .HasMaxLength(9) // e.g., "2023", "2024-2025"
            .IsRequired();

        builder.Property<int>(BowlerConfiguration.ForeignKeyName)
            .IsRequired();

        builder.Property(seasonAward => seasonAward.BowlerOfTheYearCategory)
            .HasConversion<SmartEnumConverter<BowlerOfTheYearCategory, int>>();

        builder.Property(seasonAward => seasonAward.HighBlockScore);

        builder.Property(seasonAward => seasonAward.Average)
            .HasPrecision(5, 2);

        builder.Property(seasonAward => seasonAward.SeasonTotalGames);

        builder.Property(seasonAward => seasonAward.Tournaments);


        builder.HasIndex(seasonAward => seasonAward.Id)
            .IsUnique();

        builder.HasIndex(seasonAward => seasonAward.Season);
    }
}
