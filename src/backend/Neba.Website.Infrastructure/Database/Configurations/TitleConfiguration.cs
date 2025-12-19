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
        builder.HasKey(title => title.Id);

        builder.HasIndex(title => new { title.Year, title.Month });

        builder
            .Property(title => title.Id)
            .ValueGeneratedNever()
            .HasMaxLength(26)
            .IsFixedLength()
            .HasConversion<TitleId.EfCoreValueConverter>();

        builder
            .Property(title => title.BowlerId)
            .IsRequired()
            .HasConversion<BowlerId.EfCoreValueConverter>();

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
