using System.Linq.Expressions;
using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Contact;

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
        /// <see cref="EntityTypeBuilder{T}"/> with default column naming conventions.
        /// </summary>
        /// <param name="addressExpression">Expression that points to the address property on the entity.</param>
        /// <param name="configureAddress">Optional action to override default column names or perform additional configuration (e.g., add indexes).</param>
        /// <remarks>
        /// <para>
        /// Default column names applied:
        /// <list type="bullet">
        /// <item>Street: "address_street"</item>
        /// <item>Unit: "address_unit"</item>
        /// <item>City: "address_city"</item>
        /// <item>Region: "address_region"</item>
        /// <item>PostalCode: "address_postal_code"</item>
        /// <item>Country: "address_country"</item>
        /// <item>Latitude: "address_latitude"</item>
        /// <item>Longitude: "address_longitude"</item>
        /// </list>
        /// </para>
        /// <para>
        /// Use the <paramref name="configureAddress"/> action to override column names by calling
        /// <c>address.Property(a => a.PropertyName).HasColumnName("custom_name")</c>.
        /// </para>
        /// </remarks>
        public EntityTypeBuilder<T> HasAddress(
            Expression<Func<T, Address?>> addressExpression,
            Action<ComplexPropertyBuilder<Address>>? configureAddress = null)
        {
            return builder.ComplexProperty(addressExpression, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("address_street")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.Unit)
                    .HasColumnName("address_unit")
                    .HasMaxLength(50);

                address.Property(a => a.City)
                    .HasColumnName("address_city")
                    .HasMaxLength(50)
                    .IsRequired();

                address.Property(a => a.Region)
                    .HasColumnName("address_region")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .IsRequired();

                address.Property(a => a.Country)
                    .HasColumnName("address_country")
                    .HasConversion<SmartEnumConverter<Country, string>>()
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .IsRequired();

                address.Property(a => a.PostalCode)
                    .HasColumnName("address_postal_code")
                    .HasMaxLength(10)
                    .IsRequired();

                address.ComplexProperty(a => a.Coordinates, coordinates =>
                {
                    coordinates.Property(c => c.Latitude)
                        .HasColumnName("address_latitude")
                        .IsRequired();

                    coordinates.Property(c => c.Longitude)
                        .HasColumnName("address_longitude")
                        .IsRequired();
                });

                // Allow overriding defaults and additional configuration (e.g., indexes)
                configureAddress?.Invoke(address);
            });
        }
    }
}
