using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gclopes.FinVault.Investments.Data.Configurations;

public class PositionConfiguration
    : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Symbol)
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.Source)
            .IsRequired();

        entity.HasIndex(x => new { x.Symbol, x.Source })
            .IsUnique();
    }
}