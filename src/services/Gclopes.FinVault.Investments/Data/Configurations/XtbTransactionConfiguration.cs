using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gclopes.FinVault.Investments.Data.Configurations;

public class XtbTransactionConfiguration
    : IEntityTypeConfiguration<XtbTransaction>
{
    public void Configure(EntityTypeBuilder<XtbTransaction> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedNever();

        entity.Property(x => x.Type)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.Ticker)
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.Instrument)
            .HasMaxLength(128);

        entity.Property(x => x.Comment)
            .HasMaxLength(512);

        entity.Property(x => x.Product)
            .HasMaxLength(128);
    }
}