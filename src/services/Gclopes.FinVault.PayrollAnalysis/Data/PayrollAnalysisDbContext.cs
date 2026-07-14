using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.PayrollAnalysis.Data;

public class PayrollAnalysisDbContext(DbContextOptions<PayrollAnalysisDbContext> options) : DbContext(options)
{
    public DbSet<PayrollImport> PayrollImports => Set<PayrollImport>();
    public DbSet<PayrollItem> PayrollItems => Set<PayrollItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayrollImport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceDocumentId).IsRequired();

            // Guarantees the same source PDF is never ingested twice (RabbitMQ at-least-once
            // delivery, or an accidental re-upload of the same payslip).
            entity.HasIndex(e => e.SourceDocumentId).IsUnique();

            entity.HasMany(e => e.Items)
                  .WithOne(i => i.PayrollImport)
                  .HasForeignKey(i => i.PayrollImportId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayrollItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Field).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.ReferenceMonth).HasMaxLength(7);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);

            // Decimal precision (fixing the same kind of truncation warning already handled
            // in CurrentAccount/Investments for Oracle NUMBER columns).
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.Rate).HasPrecision(9, 4);
            entity.Property(e => e.UnitValue).HasPrecision(18, 4);
        });

        base.OnModelCreating(modelBuilder);
    }
}
