using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Database.Configurations;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Infrastructure.Database.Configurations;

internal sealed class TitleConfiguration
    : IEntityTypeConfiguration<Title>
{
    public void Configure(EntityTypeBuilder<Title> builder)
    {
        builder.ToTable("tournament_titles", WebsiteDbContext.DefaultSchema);

        builder.ConfigureShadowId();

        builder
            .Property(title => title.Id)
            .IsUlid<TitleId, TitleId.EfCoreValueConverter>();

        builder.HasAlternateKey(title => title.Id);

        builder.Property(title => title.BowlerId)
            .IsUlid<BowlerId, BowlerId.EfCoreValueConverter>("bowler_id")
            .IsRequired();
    }
}
