using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Awards;
using Neba.Website.Infrastructure.Database.Converters;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class HallOfFameInductionConfiguration
    : IEntityTypeConfiguration<HallOfFameInduction>
{
    public void Configure(
        EntityTypeBuilder<HallOfFameInduction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("hall_of_fame_inductions", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder
            .Property(induction => induction.Id)
            .IsUlid<HallOfFameId, HallOfFameId.EfCoreValueConverter>();

        builder
            .HasIndex(induction => induction.Id)
            .IsUnique();

        builder.Property(induction => induction.Year)
            .HasColumnName("induction_year")
            .IsRequired();

        builder.HasStoredFile(induction => induction.Photo,
            containerColumnName: "photo_container",
            filePathColumnName: "photo_file_path",
            contentTypeColumnName: "photo_content_type",
            sizeInBytesColumnName: "photo_size_in_bytes");

        builder.Property(induction => induction.Categories)
            .HasColumnName("category")
            .HasConversion<HallOfFameCategoryValueConverter>()
            .IsRequired();

        builder.HasIndex(induction => induction.Year);

        builder.HasAlternateKey(nameof(HallOfFameInduction.Year), BowlerConfiguration.ForeignKeyName);
    }
}
