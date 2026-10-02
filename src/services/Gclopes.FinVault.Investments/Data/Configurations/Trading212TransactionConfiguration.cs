using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gclopes.FinVault.Investments.Data.Configurations;

public class Trading212TransactionConfiguration
    : IEntityTypeConfiguration<Trading212Transaction>
{
    public void Configure(EntityTypeBuilder<Trading212Transaction> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.T212Id)
            .HasMaxLength(50)
            .IsRequired();

        entity.HasIndex(x => x.T212Id)
            .IsUnique();

        entity.Property(x => x.Action)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.Isin)
            .HasMaxLength(12)
            .IsRequired();

        entity.Property(x => x.Ticker)
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();

        entity.Property(x => x.Notes)
            .HasMaxLength(1024);

        entity.Property(x => x.CurrencyPricePerShare)
            .HasMaxLength(3)
            .IsRequired();

        entity.Property(x => x.CurrencyTotal)
            .HasMaxLength(3)
            .IsRequired();

        entity.Property(x => x.CurrencyWithholdingTax)
            .HasMaxLength(3)
            .IsRequired();
        
        entity.Property(x => x.CurrencyStampDutyReserveTax)
            .HasMaxLength(3)
            .IsRequired();

        entity.Property(x => x.CurrencyCurrencyConversionFee)
            .HasMaxLength(3)
            .IsRequired();
    }
}