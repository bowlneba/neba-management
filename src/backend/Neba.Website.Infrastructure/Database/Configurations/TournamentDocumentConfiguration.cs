using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class TournamentDocumentConfiguration
    : IEntityTypeConfiguration<TournamentDocument>
{
    public void Configure(EntityTypeBuilder<TournamentDocument> builder)
    {
        builder.ToTable("tournament_documents", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder.Property(document => document.Id)
            .IsUlid<TournamentDocumentId, TournamentDocumentId.EfCoreValueConverter>();

        builder.HasAlternateKey(document => document.Id);

        builder.Property(document => document.TournamentId)
            .IsUlid<TournamentId, TournamentId.EfCoreValueConverter>(TournamentConfiguration.ForeignKeyName);

        builder.Property(document => document.DocumentType)
            .HasConversion<SmartEnumConverter<TournamentDocumentType, int>>()
            .IsRequired();

        builder.HasStoredFile(document => document.File,
            containerColumnName: "file_container",
            filePathColumnName: "file_path",
            contentTypeColumnName: "file_content_type",
            sizeInBytesColumnName: "file_size_in_bytes");

        builder.Property(document => document.UploadedAtUtc)
            .HasColumnName("uploaded_at_utc")
            .IsRequired();
    }
}
