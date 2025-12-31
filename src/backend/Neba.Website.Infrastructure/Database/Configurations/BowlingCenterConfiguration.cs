using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.BowlingCenters;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class BowlingCenterConfiguration
    : IEntityTypeConfiguration<BowlingCenter>
{
    internal static class QueryFilters
    {
        internal const string OpenCentersFilter = "OpenCentersFilter";
    }

    public void Configure(EntityTypeBuilder<BowlingCenter> builder)
    {
        builder.ToTable("bowling_centers", WebsiteDbContext.DefaultSchema);

        builder.HasQueryFilter(QueryFilters.OpenCentersFilter,
            bowlingCenter => !bowlingCenter.IsClosed);

        builder.ConfigureShadowId();

        builder.Property(bowlingCenter => bowlingCenter.Id)
            .IsUlid<BowlingCenterId, BowlingCenterId.EfCoreValueConverter>();

        builder.HasIndex(bowlingCenter => bowlingCenter.Id)
            .IsUnique();

        builder.Property(bowlingCenter => bowlingCenter.Name)
            .HasColumnName("name")
            .HasMaxLength(75)
            .IsRequired();

        builder.HasAddress(bowlingCenter => bowlingCenter.Address,
            configureAddress: address =>
            {
                address.IsRequired();

                // Override default column names
                address.Property(a => a.Street).HasColumnName("street");
                address.Property(a => a.Unit).HasColumnName("unit");
                address.Property(a => a.City).HasColumnName("city");
                address.Property(a => a.Region).HasColumnName("state");
                address.Property(a => a.PostalCode).HasColumnName("zip_code");
                address.Property(a => a.Country).HasColumnName("country");
                address.ComplexProperty(a => a.Coordinates, coordinates =>
                {
                    coordinates.Property(c => c.Latitude).HasColumnName("latitude");
                    coordinates.Property(c => c.Longitude).HasColumnName("longitude");
                });
            });

        builder.HasPhoneNumber(bowlingCenter => bowlingCenter.PhoneNumber,
            configurePhoneNumber: phoneNumber =>
            {
                phoneNumber.IsRequired();
            });

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
