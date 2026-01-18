using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Neba.Infrastructure.Database.Configurations;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S2325 // Classes should not be static


/// <summary>
/// Provides helpers for configuring a shadow integer identifier column
/// and primary key on EF Core entity type builders.
/// </summary>
/// <remarks>
/// Use the <see cref="ConfigureShadowId{T}(EntityTypeBuilder{T},string,string)"/>
/// extension to add a shadow identity column (default name "db_id")
/// mapped to a database column (default name "id") and configure it as the
/// primary key using identity semantics.
/// </remarks>
public static class ShadowIdConfiguration
{
    /// <summary>
    /// The default CLR property name for the shadow identifier.
    /// </summary>
    public const string DefaultPropertyName = "db_id";

    /// <summary>
    /// The default database column name for the shadow identifier.
    /// </summary>
    public const string DefaultColumnName = "id";

    /// <summary>
    /// Configures a shadow integer identity property on the entity and marks it as the primary key.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="builder">The <see cref="EntityTypeBuilder{T}"/> for the entity.</param>
    /// <param name="propertyName">The CLR name to use for the shadow property (default: "db_id").</param>
    /// <param name="columnName">The database column name to map the shadow property to (default: "id").</param>
    public static void ConfigureShadowId<T>(
        this EntityTypeBuilder<T> builder,
        string propertyName = DefaultPropertyName,
        string columnName = DefaultColumnName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property<int>(propertyName)
            .HasColumnName(columnName)
            .UseIdentityAlwaysColumn();

        builder.HasKey(propertyName);
    }
}
