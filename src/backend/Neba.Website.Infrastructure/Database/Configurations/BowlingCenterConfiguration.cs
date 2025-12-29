using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.BowlingCenters;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class BowlingCenterConfiguration
    : IEntityTypeConfiguration<BowlingCenter>
{
    public void Configure(EntityTypeBuilder<BowlingCenter> builder)
    {
        builder.ToTable("bowling_centers", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder.Property(bowlingCenter => bowlingCenter.Id)
            .IsUlid<BowlingCenterId, BowlingCenterId.EfCoreValueConverter>();

        builder.HasIndex(bowlingCenter => bowlingCenter.Id)
            .IsUnique();

        builder.Property(bowlingCenter => bowlingCenter.Name)
            .HasColumnName("name")
            .HasMaxLength(75)
            .IsRequired();

        builder.OwnsAddress(bowlingCenter => bowlingCenter.Address,
            streetColumnName: "street",
            unitColumnName: "unit",
            cityColumnName: "city",
            regionColumnName: "state",
            postalCodeColumnName: "zip_code",
            countryColumnName: "country",
            latitudeColumnName: "latitude",
            longitudeColumnName: "longitude")
            .Navigation(bowlingCenter => bowlingCenter.Address)
            .IsRequired();

        builder.Property(bowlingCenter => bowlingCenter.IsClosed)
            .HasColumnName("closed")
            .IsRequired();

        builder.Property(bowlingCenter => bowlingCenter.WebsiteId)
            .ValueGeneratedNever();

        builder.HasIndex(bowlingCenter => bowlingCenter.WebsiteId)
            .IsUnique()
            .HasFilter("\"website_id\" IS NOT NULL");

        builder.Property(bowlingCenter => bowlingCenter.ApplicationId)
            .ValueGeneratedNever();

        builder.HasIndex(bowlingCenter => bowlingCenter.ApplicationId)
            .IsUnique()
            .HasFilter("\"application_id\" IS NOT NULL");
    }
}
