using Ardalis.SmartEnum.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain;
using Neba.Domain.Bowlers;
using Neba.Domain.Tournaments;

namespace Neba.Infrastructure.Database.Website.Configurations;

internal sealed class TitleConfiguration
    : IEntityTypeConfiguration<Title>
{
    public void Configure(EntityTypeBuilder<Title> builder)
    {
        builder.ToTable("titles", "website");
        builder.HasKey(title => title.Id);

        builder.HasIndex(title => new { title.Year, title.Month });

        builder
            .Property(title => title.Id)
            .ValueGeneratedNever()
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
