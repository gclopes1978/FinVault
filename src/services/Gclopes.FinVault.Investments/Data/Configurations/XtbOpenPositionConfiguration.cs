using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gclopes.FinVault.Investments.Data.Configurations;

public class XtbOpenPositionConfiguration
    : IEntityTypeConfiguration<XtbOpenPosition>
{
    public void Configure(EntityTypeBuilder<XtbOpenPosition> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Product)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.InstrumentPosition)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.Ticker)
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.Category)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.Type)
            .HasMaxLength(64)
            .IsRequired();

        entity.HasIndex(x => x.Ticker);

        entity.HasIndex(x => x.OpenTimeUtc);
    }
}