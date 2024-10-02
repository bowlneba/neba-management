using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Neba.Infrastructure.Persistence;

/// <summary>
/// Represents an audit entry.
/// </summary>
public sealed class AuditEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit entry.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for the transaction.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Gets or sets the metadata associated with the audit entry.
    /// </summary>
    public required string Metadata { get; init; }

    /// <summary>
    /// Gets or sets the start time of the audit entry in UTC.
    /// </summary>
    public DateTime StartTimeUtc { get; init; }

    /// <summary>
    /// Gets or sets the end time of the audit entry in UTC.
    /// </summary>
    public DateTime EndTimeUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Configures the <see cref="AuditEntry"/> entity.
/// </summary>
internal sealed class AuditEntryConfiguration
    : IEntityTypeConfiguration<AuditEntry>
{
    /// <summary>
    /// Configures the <see cref="AuditEntry"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("Audit");

        builder.HasKey(audit => audit.Id);

        builder.HasIndex(audit => audit.TransactionId);

        builder.Property(audit => audit.TransactionId)
            .IsRequired();

        builder.Property(audit => audit.Metadata)
            .IsRequired();

        builder.Property(audit => audit.StartTimeUtc)
            .IsRequired();

        builder.Property(audit => audit.EndTimeUtc)
            .IsRequired();

        builder.Property(audit => audit.Succeeded)
            .IsRequired();

        builder.Property(audit => audit.ErrorMessage)
            .HasMaxLength(4000);
    }
}