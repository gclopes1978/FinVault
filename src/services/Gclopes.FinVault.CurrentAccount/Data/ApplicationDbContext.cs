using Gclopes.FinVault.CurrentAccount.Models;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.CurrentAccount.Data
{
    // Define the core application database context class managing tracking data mappings
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {

        // Map the structural AuditLog data model out into a dedicated tracking database table schema
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        // Map the baseline financial transaction account movements records database table schema
        public DbSet<BankStatement> BankStatements => Set<BankStatement>();
        public DbSet<Movement> Movements => Set<Movement>();

        // Override configuring pipeline structures to register independent system interceptor hooks
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            // Execute default baseline lifecycle configurations inside the base database component framework
            base.OnConfiguring(optionsBuilder);
        }

        // Configure explicit model constraint criteria configurations during initial creation assembly execution
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ChangesPayload).IsRequired();
            });

            modelBuilder.Entity<BankStatement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InitialBalance).HasPrecision(18, 2);
                entity.Property(e => e.FinalBalance).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Movement>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(1);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(m => m.BankStatement)
                      .WithMany(s => s.Movements)
                      .HasForeignKey(m => m.BankStatementId) // Certifica-te que esta propriedade existe em Movement.cs
                      .OnDelete(DeleteBehavior.Cascade); // Recomendado para evitar órfãos

                // Speeds up the default TransactionDate desc, ValueDate desc sort and the
                // date-range filters used by GET /api/movements and /api/movements/totals
                entity.HasIndex(m => new { m.TransactionDate, m.ValueDate });

                // Speeds up SUM(...) WHERE Type = 'C'/'D' in the totals endpoint
                entity.HasIndex(m => m.Type);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}