using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class TitleConfiguration
    : IEntityTypeConfiguration<Title>
{
    public void Configure(EntityTypeBuilder<Title> builder)
    {
        builder.ToTable("titles", WebsiteDbContext.DefaultSchema);

        builder.Property<int>("db_id")
            .HasColumnName("db_id")
            .ValueGeneratedOnAdd();

        builder.HasKey("db_id");

        builder.HasIndex(title => new { title.Year, title.Month });

        builder
            .Property(title => title.Id)
            .HasColumnName("title_id")
            .ValueGeneratedNever()
            .HasMaxLength(26)
            .IsFixedLength()
            .HasConversion<TitleId.EfCoreValueConverter>();

        builder.HasIndex(title => title.Id)
            .IsUnique();

        builder.Property<int>(BowlerConfiguration.ShadowForeignKeyName)
            .IsRequired();

        builder
            .Property(title => title.TournamentType)
            .HasConversion<SmartEnumConverter<TournamentType, int>>()
            .IsRequired();

        builder
            .Property(title => title.Month)
            .HasConversion<SmartEnumConverter<Month, int>>()
            .IsRequired();

        builder
            .Property(title => title.Year)
            .IsRequired();
    }
}
