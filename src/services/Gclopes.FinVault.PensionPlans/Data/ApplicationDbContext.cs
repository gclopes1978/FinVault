using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.PensionPlans.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<PensionInsuranceMovementEntity> PensionInsuranceMovement => Set<PensionInsuranceMovementEntity>();

        public DbSet<PensionFundMovementEntity> PensionFundMovement => Set<PensionFundMovementEntity>();
        // Override configuring pipeline structures to register independent system interceptor hooks
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            // Execute default baseline lifecycle configurations inside the base database component framework
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PensionInsuranceMovementEntity>(entity =>
            {
                entity.ToTable("PensionInsuranceMovements");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Movement).IsRequired().HasMaxLength(128);
                entity.Property(e => e.Receipt).HasMaxLength(100);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Debit).HasPrecision(18, 2);
                entity.Property(e => e.Credit).HasPrecision(18, 2);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            modelBuilder.Entity<PensionFundMovementEntity>(entity =>
            {
                entity.ToTable("PensionFundMovements");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Quantity).HasPrecision(18, 4);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
                entity.Property(e => e.Valuation).HasPrecision(18, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
