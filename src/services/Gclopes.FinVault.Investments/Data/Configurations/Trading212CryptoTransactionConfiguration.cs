using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gclopes.FinVault.Investments.Data.Configurations;

/// <summary>
/// Configures the Trading212 cryptocurrency transaction entity.
/// </summary>
public class Trading212CryptoTransactionConfiguration
    : IEntityTypeConfiguration<Trading212CryptoTransaction>
{
    public void Configure(
        EntityTypeBuilder<Trading212CryptoTransaction> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ExecutionId)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.OrderId)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.AssetSymbol)
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.Direction)
            .HasMaxLength(16)
            .IsRequired();

        entity.HasIndex(x => x.ExecutionId)
            .IsUnique();
    }
}