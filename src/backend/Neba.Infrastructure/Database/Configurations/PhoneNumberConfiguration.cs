using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Contact;

namespace Neba.Infrastructure.Database.Configurations;

#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable S2325 // Unused private types or members should be static
#pragma warning disable CA1034 // Nested types should not be visible

/// <summary>
/// Helper extensions for configuring the <see cref="PhoneNumber"/> value object
/// on Entity Framework Core entity types.
/// </summary>
/// <remarks>
/// Use the <see cref="HasPhoneNumber{T}(EntityTypeBuilder{T}, System.Linq.Expressions.Expression{System.Func{T, PhoneNumber?}}, System.Action{Microsoft.EntityFrameworkCore.Metadata.Builders.ComplexPropertyBuilder{PhoneNumber}}?)"/>
/// extension to apply a consistent mapping for the phone number's parts (country code,
/// number and extension) across different entities. This class contains only extension
/// helpers and does not hold state.
/// </remarks>
public static class PhoneNumberConfiguration
{
    /// <summary>
    /// Configures an owned/complex <see cref="PhoneNumber"/> property on an entity type.
    /// Applies consistent column names and length constraints for the phone number parts
    /// (country code, number and extension) and allows additional configuration via a callback.
    /// </summary>
    /// <remarks>
    /// This is an extension method for <see cref="EntityTypeBuilder{T}"/> so it can be used fluently
    /// inside <c>OnModelCreating</c> or IEntityTypeConfiguration implementations.
    /// </remarks>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="builder">The <see cref="EntityTypeBuilder{T}"/> for the entity.</param>
    /// <param name="phoneNumberExpression">An expression selecting the <see cref="PhoneNumber"/> property on the entity.</param>
    /// <param name="configurePhoneNumber">Optional additional configuration action for the complex property.</param>
    /// <returns>The same <see cref="EntityTypeBuilder{T}"/> instance to allow fluent chaining.</returns>
    public static EntityTypeBuilder<T> HasPhoneNumber<T>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, PhoneNumber?>> phoneNumberExpression,
        Action<ComplexPropertyBuilder<PhoneNumber>>? configurePhoneNumber = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ComplexProperty(phoneNumberExpression, phoneNumber =>
        {
            phoneNumber.Property(p => p.CountryCode)
                .HasColumnName("phone_country_code")
                .HasMaxLength(3)
                .IsRequired();

            phoneNumber.Property(p => p.Number)
                .HasColumnName("phone_number")
                .HasMaxLength(15)
                .IsRequired();

            phoneNumber.Property(p => p.Extension)
                .HasColumnName("phone_extension")
                .HasMaxLength(10);

            configurePhoneNumber?.Invoke(phoneNumber);
        });
    }
}
