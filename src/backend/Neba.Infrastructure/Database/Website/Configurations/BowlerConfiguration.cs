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
                    .HasIndex(name => new { name.LastName, name.FirstName });

                nameBuilder
                    .Property(name => name.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(20)
                    .IsRequired();

                nameBuilder
                    .Property(name => name.MiddleInitial)
                    .HasColumnName("middle_initial")
                    .HasMaxLength(1)
                    .IsFixedLength();

                nameBuilder
                    .Property(name => name.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsRequired();

                nameBuilder
                    .Property(name => name.Suffix)
                    .HasColumnName("suffix")
                    .HasMaxLength(5);

                nameBuilder
                    .Property(name => name.Nickname)
                    .HasColumnName("nickname")
                    .HasMaxLength(30);
            });

        builder.Property(bowler => bowler.WebsiteId)
            .ValueGeneratedNever();

        builder.Property(bowler => bowler.ApplicationId)
            .ValueGeneratedNever();
    }
}
