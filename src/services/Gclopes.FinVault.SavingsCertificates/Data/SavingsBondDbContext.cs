using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.SavingsCertificates.Data;

// FIX: This DbContext was missing from the repository entirely (see SavingsBond.cs
// for details). Recreated here so it matches exactly the schema already recorded in
// Migrations/SavingsBondDbContextModelSnapshot.cs, keeping the existing migration valid.
public class SavingsBondDbContext(DbContextOptions<SavingsBondDbContext> options) : DbContext(options)
{
    public DbSet<SavingsBond> SavingsBonds => Set<SavingsBond>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SavingsBond>(entity =>
        {
            entity.ToTable("SavingsBonds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Series).IsRequired().HasMaxLength(1);
            entity.Property(e => e.BaseRate).HasPrecision(18, 5);
            entity.Property(e => e.SubscriptionDate).HasColumnType("date");
            entity.Property(e => e.UnitValue).HasPrecision(18, 5);

            // FIX: Quantity is an int, so it must NOT have HasPrecision applied to it
            // (that's only valid for decimal columns). The original migration snapshot
            // had HasPrecision(18, 5) copy-pasted onto this int property; intentionally
            // not repeated here, and removed from the migration/snapshot files too.
        });

        base.OnModelCreating(modelBuilder);
    }
}
