using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gclopes.FinVault.Investments.Data.Configurations;

public class XtbCashOperationConfiguration
    : IEntityTypeConfiguration<XtbCashOperation>
{
    public void Configure(EntityTypeBuilder<XtbCashOperation> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Type)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.Instrument)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.Ticker)
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.Category)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.OperationId)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.Comment)
            .HasMaxLength(1024);

        entity.Property(x => x.Product)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.PositionId)
            .HasMaxLength(128);

        entity.HasIndex(x => x.OperationId)
            .IsUnique();

        entity.HasIndex(x => x.Ticker);

        entity.HasIndex(x => x.Time);
    }
}