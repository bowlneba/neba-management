using System.Linq.Expressions;
using Ardalis.SmartEnum.SystemTextJson;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Addresses;

namespace Neba.Infrastructure.Database.Configurations;

#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable S2325 // Unused private types or members should be static
#pragma warning disable CA1034 // Nested types should not be visible

/// <summary>
/// Helper methods to configure how the <see cref="Address"/> value object is mapped
/// to database columns when used as an owned type by entity types.
/// </summary>
public static class AddressConfiguration
{
    extension<T>(EntityTypeBuilder<T> builder)
        where T : class
    {
        /// <summary>
    /// Adds an owned mapping for an <see cref="Address"/> value object to the provided
    /// <see cref="EntityTypeBuilder{T}"/>. Column names and precision for coordinates can be customized.
    /// </summary>
    /// <param name="addressExpression">Expression that points to the address property on the entity.</param>
    /// <param name="streetColumnName">Column name for the street field. Default: "address_street".</param>
    /// <param name="unitColumnName">Column name for the unit field. Default: "address_unit".</param>
    /// <param name="cityColumnName">Column name for the city field. Default: "address_city".</param>
    /// <param name="regionColumnName">Column name for the region/state field. Default: "address_region".</param>
    /// <param name="postalCodeColumnName">Column name for the postal code field. Default: "address_postal_code".</param>
    /// <param name="countryColumnName">Column name for the country field. Default: "address_country".</param>
    /// <param name="latitudeColumnName">Column name for latitude. Default: "address_latitude".</param>
    /// <param name="longitudeColumnName">Column name for longitude. Default: "address_longitude".</param>
    /// <remarks>
    /// This helper centralizes column naming and precision decisions for persistence of the
    /// <see cref="Address"/> value object so multiple entity configurations stay consistent.
    /// </remarks>
        public void OwnsAddress(
            Expression<Func<T, Address?>> addressExpression,
            string streetColumnName = "address_street",
            string unitColumnName = "address_unit",
            string cityColumnName = "address_city",
            string regionColumnName = "address_region",
            string postalCodeColumnName = "address_postal_code",
            string countryColumnName = "address_country",
            string latitudeColumnName = "address_latitude",
            string longitudeColumnName = "address_longitude")
        {
            builder.OwnsOne(addressExpression, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName(streetColumnName)
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.Unit)
                    .HasColumnName(unitColumnName)
                    .HasMaxLength(50);

                address.Property(a => a.City)
                    .HasColumnName(cityColumnName)
                    .HasMaxLength(50)
                    .IsRequired();

                address.Property(a => a.Region)
                    .HasColumnName(regionColumnName)
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .IsRequired();

                address.Property(a => a.Country)
                    .HasColumnName(countryColumnName)
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .HasConversion<SmartEnumValueConverter<Country, string>>()
                    .IsRequired();

                address.Property(a => a.PostalCode)
                    .HasColumnName(postalCodeColumnName)
                    .HasMaxLength(10)
                    .IsRequired();

                address.OwnsOne(a => a.Coordinates, coordinates =>
                {
                    coordinates.Property(c => c.Latitude)
                        .HasColumnName(latitudeColumnName)
                        .HasPrecision(8,6)
                        .IsRequired();

                    coordinates.Property(c => c.Longitude)
                        .HasColumnName(longitudeColumnName)
                        .HasPrecision(9,6)
                        .IsRequired();
                });
            });
        }
    }
}
