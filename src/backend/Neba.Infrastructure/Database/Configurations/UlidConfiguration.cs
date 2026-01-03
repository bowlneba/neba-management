using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Neba.Infrastructure.Database.Configurations;

#pragma warning disable CA1034 // Nested types should not be visible

/// <summary>
/// Provides extension methods to configure ULID-based identifier properties for EF Core entities.
/// </summary>
/// <remarks>
/// This helper centralizes the common configuration used for ULID-like identifiers persisted as
/// fixed-length 26-character strings in the database. It configures the column name, length,
/// and conversion using the provided EF Core value converter type.
/// </remarks>
public static class UlidConfiguration
{
    /// <summary>
    /// Configures a <see cref="PropertyBuilder{TId}"/> instance to map an identifier that uses a
    /// ULID-style string representation in the database.
    /// </summary>
    /// <typeparam name="TId">The CLR type of the identifier (usually a struct or value type wrapping a ULID).</typeparam>
    /// <typeparam name="TEfCoreValueConverter">The EF Core value converter type used to convert between <typeparamref name="TId"/> and the provider type (for example, a <c>ValueConverter&lt;TId, string&gt;</c>).</typeparam>
    /// <param name="propertyBuilder">The <see cref="PropertyBuilder{TId}"/> instance to configure; this is the extension method receiver.</param>
    /// <param name="columnName">The database column name to use for the identifier. Defaults to <c>"domain_id"</c>.</param>
    /// <returns>The same <see cref="PropertyBuilder{TId}"/> after applying the ULID configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyBuilder"/> is <see langword="null"/>.</exception>
    public static PropertyBuilder<TId> IsUlid<TId, TEfCoreValueConverter>(
        this PropertyBuilder<TId> propertyBuilder,
        string columnName = "domain_id")
        where TId : struct
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        return propertyBuilder
            .HasColumnName(columnName)
            .HasMaxLength(26)
            .IsFixedLength()
            .ValueGeneratedNever()
            .HasConversion<TEfCoreValueConverter>();
    }

    /// <summary>
    /// Configures a <see cref="PropertyBuilder{TId}"/> instance to map an identifier that uses a
    /// ULID-style string representation in the database.
    /// </summary>
    /// <typeparam name="TId">The CLR type of the identifier (usually a struct or value type wrapping a ULID).</typeparam>
    /// <typeparam name="TEfCoreValueConverter">The EF Core value converter type used to convert between <typeparamref name="TId"/> and the provider type (for example, a <c>ValueConverter&lt;TId, string&gt;</c>).</typeparam>
    /// <param name="propertyBuilder">The <see cref="PropertyBuilder{TId}"/> instance to configure; this is the extension method receiver.</param>
    /// <param name="columnName">The database column name to use for the identifier. Defaults to <c>"domain_id"</c>.</param>
    /// <returns>The same <see cref="PropertyBuilder{TId}"/> after applying the ULID configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyBuilder"/> is <see langword="null"/>.</exception>
    public static PropertyBuilder<TId?> IsUlid<TId, TEfCoreValueConverter>(
        this PropertyBuilder<TId?> propertyBuilder,
        string columnName = "domain_id")
        where TId : struct
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        return propertyBuilder
            .HasColumnName(columnName)
            .HasMaxLength(26)
            .IsFixedLength()
            .ValueGeneratedNever()
            .HasConversion<TEfCoreValueConverter>();
    }
}
