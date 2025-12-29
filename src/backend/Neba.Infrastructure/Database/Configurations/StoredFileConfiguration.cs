using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Neba.Infrastructure.Database.Configurations;

/// <summary>
/// Provides EF Core mapping helpers for the <see cref="Domain.StoredFile"/> value object.
/// </summary>
public static class StoredFileConfiguration
{
    /// <summary>
    /// Configures an owned <see cref="Domain.StoredFile"/> on the entity type and maps
    /// its properties to individual columns with sensible defaults.
    /// </summary>
    /// <typeparam name="T">The entity type that owns the stored file value object.</typeparam>
    /// <param name="builder">The <see cref="EntityTypeBuilder{T}"/> for the owning entity.</param>
    /// <param name="fileExpression">An expression pointing to the owned <see cref="Domain.StoredFile"/> property.</param>
    /// <param name="containerColumnName">Database column name for the file location (default: "file_location").</param>
    /// <param name="filePathColumnName">Database column name for the file name (default: "file_name").</param>
    /// <param name="contentTypeColumnName">Database column name for the content type (default: "file_content_type").</param>
    /// <param name="sizeInBytesColumnName">Database column name for the file size in bytes (default: "file_size_in_bytes").</param>
    public static void OwnsStoredFile<T>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, Domain.StoredFile?>> fileExpression,
        string containerColumnName = "file_container",
        string filePathColumnName = "file_path",
        string contentTypeColumnName = "file_content_type",
        string sizeInBytesColumnName = "file_size_in_bytes")
            where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.OwnsOne(fileExpression, file =>
        {
            file.Property(f => f.Container)
                .HasColumnName(containerColumnName)
                .HasMaxLength(63)
                .IsRequired();

            file.Property(f => f.Path)
                .HasColumnName(filePathColumnName)
                .HasMaxLength(1024)
                .IsRequired();

            file.Property(f => f.ContentType)
                .HasColumnName(contentTypeColumnName)
                .HasMaxLength(255)
                .IsRequired();

            file.Property(f => f.SizeInBytes)
                .HasColumnName(sizeInBytesColumnName)
                .IsRequired();
        });
    }
}
