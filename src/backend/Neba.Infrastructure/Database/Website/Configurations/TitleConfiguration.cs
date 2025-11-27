using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            .HasColumnName("tournament_type")
            .IsRequired();

        builder
            .Property(title => title.Month)
            .HasColumnName("month")
            .IsRequired();

        builder
            .Property(title => title.Year)
            .HasColumnName("year")
            .IsRequired();
    }
}
