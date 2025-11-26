using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neba.Domain.Bowlers;

namespace Neba.Infrastructure.Database.Website.Configurations;

internal sealed class BowlerConfiguration
    : IEntityTypeConfiguration<Bowler>
{
    public void Configure(EntityTypeBuilder<Bowler> builder)
    {
        builder.ToTable("bowlers", "website");
        builder.HasKey(bowler => bowler.Id);

        builder
            .Property(bowler => bowler.Id)
            .ValueGeneratedNever()
            .HasConversion<BowlerId.EfCoreValueConverter>();

        builder
            .OwnsOne(bowler => bowler.Name, nameBuilder =>
            {
                nameBuilder
                    .Property(name => name.FirstName)
                    .HasMaxLength(20)
                    .IsRequired();

                nameBuilder
                    .Property(name => name.LastName)
                    .HasMaxLength(30)
                    .IsRequired();

                nameBuilder
                    .Property(name => name.MiddleInitial)
                    .HasMaxLength(1)
                    .IsFixedLength();

                nameBuilder
                    .Property(name => name.Suffix)
                    .HasMaxLength(5);

                nameBuilder
                    .Property(name => name.Nickname)
                    .HasMaxLength(30);

            });
    }
}
