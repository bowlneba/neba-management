using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class TournamentFileConfiguration
    : IEntityTypeConfiguration<TournamentFile>
{
    public void Configure(EntityTypeBuilder<TournamentFile> builder)
    {
        builder.ToTable("tournament_files", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder.Property(file => file.Id)
            .IsUlid<TournamentFileId, TournamentFileId.EfCoreValueConverter>();

        builder.HasAlternateKey(file => file.Id);

        builder.Property(file => file.FileType)
            .HasConversion<SmartEnumConverter<TournamentFileType, int>>()
            .IsRequired();

        builder.HasStoredFile(file => file.File,
            containerColumnName: "file_container",
            filePathColumnName: "file_path",
            contentTypeColumnName: "file_content_type",
            sizeInBytesColumnName: "file_size_in_bytes");

        builder.Property(file => file.UploadedAtUtc)
            .HasColumnName("uploaded_at_utc")
            .IsRequired();
    }
}
