using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class TitleConfiguration
    : IEntityTypeConfiguration<Title>
{
    public void Configure(EntityTypeBuilder<Title> builder)
    {
        builder.ToTable("titles", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder.HasIndex(title => new { title.Year, title.Month });

        builder
            .Property(title => title.Id)
            .IsUlid<TitleId, TitleId.EfCoreValueConverter>();

        builder.HasIndex(title => title.Id)
            .IsUnique();

        builder.Property<int>(BowlerConfiguration.ForeignKeyName)
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
