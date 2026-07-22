using Gclopes.FinVault.DocumentIngestion.Models;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.DocumentIngestion.Data;

// Application database context for the DocumentIngestion service. Currently only tracks
// UploadedDocument rows (the import history shown in the PDF Upload Centre); the raw PDF bytes
// themselves still live on disk under StorageSettings:Path, not in the database.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UploadedDocument> UploadedDocuments => Set<UploadedDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UploadedDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(260);
            entity.Property(e => e.DocumentType).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.UploadedAt).IsRequired();
            entity.HasIndex(e => e.UploadedAt);
        });

        base.OnModelCreating(modelBuilder);
    }
}
